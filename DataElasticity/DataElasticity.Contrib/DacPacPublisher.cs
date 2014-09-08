#region usings

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.SqlServer.Dac;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Contrib
{
    //TODO: I need to have the progress and messages actively written to 
    //      the log window that I have open. (This would be interesting to
    //      see resolved).  In the meantime I can send the deploy messages
    //      and progress to a sql location (seems inefficient).  In the future
    //      I will want these things to be written to the same destination
    //      as the telemetry.


    public class DacPacPublisher : PublisherBase
    {
        #region fields

        private readonly ShardConnection _masterCatalogConnection;
        private readonly ShardConnection _shardCatalogConnection;
        private readonly string _shardUserName;
        private readonly string _shardUserPassword;
        private readonly ShardSetConfig _shardSetConfig;

        #endregion

        #region properties

        private bool IsAzureDb
        {
            get { return _masterCatalogConnection.ConnectionString.ToLower().Contains("database.windows.net"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DacPacPublisher"/> class.
        /// </summary>
        /// <param name="parameters">The dacpac publisher parameters.</param>
        public DacPacPublisher(DacPacPublisherParams parameters)
        {
            _shardSetConfig = parameters.ShardSetConfig;
            _shardUserName = parameters.ShardUserName;
            _shardUserPassword = parameters.ShardUserPassword;

            // create connection object to the shard instance, shard catalog with admin credentials
            _shardCatalogConnection =
                new ShardConnection
                {
                    ServerInstanceName = parameters.Shard.ServerInstanceName,
                    Catalog = parameters.Shard.Catalog,
                    UserName = parameters.ShardAdminUserName,
                    Password = parameters.ShardAdminPassword,
                    ShardSetName = parameters.ShardSetConfig.ShardSetName
                };

            // create connection object to the shard instance, master catalog with admin credentials
            _masterCatalogConnection =
                new ShardConnection
                {
                    ServerInstanceName = parameters.Shard.ServerInstanceName,
                    Catalog = "master",
                    UserName = parameters.ShardAdminUserName,
                    Password = parameters.ShardAdminPassword,
                    ShardSetName = parameters.ShardSetConfig.ShardSetName
                };
        }

        #endregion

        #region methods

        public void DropDatabase()
        {
            using (var connection = GetReliableConnection(_masterCatalogConnection.ConnectionString))
            {
                connection.Open();

                try
                {
                    var catalog = _shardCatalogConnection.Catalog;

                    if (!IsAzureDb)
                    {
                        //Set the database to single user mode (drops all other connections)
                        var singleUserModeDbCommand = connection.CreateCommand();
                        singleUserModeDbCommand.CommandText =
                            string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", catalog);
                        singleUserModeDbCommand.ExecuteNonQuery();
                    }

                    //Drop the database now that we are the only one connected to it
                    var dropDbCommand = connection.CreateCommand();
                    dropDbCommand.CommandText = string.Format("DROP DATABASE [{0}]", catalog);
                    dropDbCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // todo: Add Logging
                    throw;
                }
            }
        }

        /// <summary>
        /// Publishes the DacPac to the shard provided in the constructor.
        /// </summary>
        /// <param name="dacPacPath">The dac pac path.</param>
        /// <param name="dacProfilePath">The dac profile path.</param>
        /// <returns><c>true</c> if a new database was created, <c>false</c> otherwise.</returns>
        public bool PublishDacPac(string dacPacPath, string dacProfilePath)
        {
            // read the DacPac package and profile from files
            var dacPackage = DacPackage.Load(dacPacPath);
            var dacProfile = DacProfile.Load(dacProfilePath);

            return PublishDacPac(dacPackage, dacProfile);
        }

        /// <summary>
        /// Publishes the DacPac to the shard provided in the constructor.
        /// </summary>
        /// <param name="dacPacStream">The dac pac stream.</param>
        /// <param name="dacProfileStream">The dac profile stream.</param>
        /// <returns><c>true</c> if a new database was created, <c>false</c> otherwise.</returns>
        public bool PublishDacPac(Stream dacPacStream, Stream dacProfileStream)
        {
            // read the DacPac package and profile from files
            var dacPackage = DacPackage.Load(dacPacStream);
            var dacProfile = DacProfile.Load(dacProfileStream);

            return PublishDacPac(dacPackage, dacProfile);
        }

        /// <summary>
        /// Publishes the DacPac to the shard provided in the constructor.
        /// </summary>
        /// <param name="dacPackage">The dac package.</param>
        /// <param name="dacProfile">The dac profile.</param>
        /// <returns><c>true</c> if a new database was created, <c>false</c> otherwise.</returns>
        public bool PublishDacPac(DacPackage dacPackage, DacProfile dacProfile)
        {
            try
            {
                // instantiate a DacPac service
                var dacServices = new DacServices(_shardCatalogConnection.ConnectionString);
                dacServices.Message += ReceiveDacServiceMessage;
                dacServices.ProgressChanged += ReceiveDacServiceProgressEvent;

                // extract the options from the profile
                var dacDeployOptions = dacProfile.DeployOptions;

                // determine if a new database will be created
                var isNewDb =
                    dacDeployOptions.CreateNewDatabase
                    || !ShardedDatabaseExists();

                // create the Dac Pac package by loading the DacPac package
                using (var package = dacPackage)
                {
                    // Deploy the Dac Pac package by loading the DacPac package
                    const bool upgradeExistingDb = true;
                    dacServices.Deploy(package, _shardCatalogConnection.Catalog, upgradeExistingDb,
                        dacDeployOptions);
                }

                //execute post deployment actions to support scale out sharding
                PostPublishDacPacActions(isNewDb);

                return isNewDb;
            }
            catch (Exception e)
            {
                // todo: Add Logging
                //Logger.WriteToLogText("Failed PublishDacpac: {0} {1} {2}", ex.Source, ex.Message, ex.StackTrace);
                throw;
            }
        }

        private void AlterDbSize()
        {
            if (IsAzureDb)
                // The ALTER DATABASE for Azure is syntactically and functionally different from on-premise.
            {
                var sizeGb = _shardSetConfig.MinShardSizeMb/1024;

                //{1 | 5 | 10 | 20 | 30 … 150} GB) 
                if (sizeGb <= 1)
                    sizeGb = 1;
                else if (sizeGb <= 5)
                    sizeGb = 5;
                else if (sizeGb <= 150)
                    sizeGb = (Int32) (Math.Ceiling((decimal) (sizeGb/10))*10);
                else
                    throw new ArgumentOutOfRangeException();

                DoAlterDbSize(sizeGb);
            }
            else
            {
                DoAlterDbSize(_shardSetConfig.MinShardSizeMb, _shardSetConfig.MaxShardSizeMb);
            }
        }

        private void ApplyPermissionsToShardUser()
        {
            //Go through all the User-Defined SP's found in the database and assign execute 
            //permissions to shardUser
            var spNames = GetSpNames(_shardCatalogConnection.ConnectionString);
            var sqlCmdText = "";

            foreach (var sp in spNames)
            {
                sqlCmdText += String.Format("GRANT EXECUTE ON OBJECT::{0} TO {1}", sp, _shardUserName) +
                              Environment.NewLine;
            }

            sqlCmdText += String.Format("EXEC sp_addrolemember 'db_datareader', '{0}';{1}", _shardUserName,
                Environment.NewLine);
            sqlCmdText += String.Format("EXEC sp_addrolemember 'db_datawriter', '{0}';{1}", _shardUserName,
                Environment.NewLine);

            //sqlCmdText += String.Format("ALTER ROLE db_datareader ADD MEMBER {0};{1}", _shardUserName,
            //    Environment.NewLine);
            //sqlCmdText += String.Format("ALTER ROLE db_datawriter ADD MEMBER {0};{1}", _shardUserName,
            //    Environment.NewLine);

            using (var conn = GetReliableConnection(_shardCatalogConnection.ConnectionString))
            {
                conn.Open();

                try
                {
                    var command = conn.CreateCommand();
                    command.CommandText = sqlCmdText;

                    command.ExecuteNonQueryWithRetry();
                }
                catch (Exception)
                {
                    // todo: add Logging
                    throw;
                }
            }
        }

        private void CreateShardUser()
        {
            if (IsAzureDb)
            {
                CreateUserAzure();
            }
            else
            {
                CreateUserOnPremise();
            }
        }

        private void CreateUserAzure()
        {
            using (var connection = GetReliableConnection(_masterCatalogConnection.ConnectionString))
            {
                connection.Open();

                try
                {
                    var loginCommand = connection.CreateCommand();

                    loginCommand.CommandText =
                        String.Format(
                            "IF NOT EXISTS(SELECT 1 FROM sys.sql_logins WHERE [name] = '{0}') (SELECT CAST(1 AS bit)) ELSE (SELECT CAST(0 AS bit));",
                            _shardUserName);

                    var shouldAddLogin = (Boolean) loginCommand.ExecuteScalarWithRetry();

                    if (shouldAddLogin)
                    {
                        loginCommand.CommandText = String.Format("CREATE LOGIN {0} WITH PASSWORD = '{1}';", _shardUserName,
                            _shardUserPassword);

                        loginCommand.ExecuteNonQueryWithRetry();
                    }
                }
                catch (Exception) //TODO: Catch & ignore if login already exists to address race condition
                {
                    throw;
                }
            }

            //Need a different connection to connect to DB so that user can be created
            using (var connection = GetReliableConnection(_shardCatalogConnection.ConnectionString))
            {
                connection.Open();

                try
                {
                    var userCommand = connection.CreateCommand();

                    userCommand.CommandText =
                        String.Format(
                            "IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name = N'{0}') (SELECT CAST(1 AS bit)) ELSE (SELECT CAST(0 AS bit));",
                            _shardUserName);

                    var shouldAddUser = (Boolean) userCommand.ExecuteScalarWithRetry();

                    if (shouldAddUser)
                    {
                        userCommand.CommandText = String.Format("CREATE USER {0} FOR LOGIN {0};", _shardUserName);
                        userCommand.ExecuteNonQueryWithRetry();
                    }
                }
                catch (Exception) //TODO: Catch & ignore if login already exists to address race condition
                {
                    throw;
                }
            }
        }

        private void CreateUserOnPremise()
        {
            using (var conn = GetReliableConnection(_masterCatalogConnection.ConnectionString))
            {
                conn.Open();

                try
                {
                    var sqlCmd = conn.CreateCommand();

                    sqlCmd.CommandText =
                        String.Format(
									 "IF NOT EXISTS(SELECT 1 FROM sys.sql_logins WHERE [name] = '{0}') (SELECT CAST(1 AS bit)) ELSE (SELECT CAST(0 AS bit));",
                            _shardUserName);

                    var addLogin = (Boolean) sqlCmd.ExecuteScalarWithRetry();

                    if (addLogin)
                    {
                        sqlCmd.CommandText = String.Format("CREATE LOGIN {0} WITH PASSWORD = '{1}';", _shardUserName,
                            _shardUserPassword);
                        sqlCmd.ExecuteNonQueryWithRetry();
                    }

                    conn.ChangeDatabase(_shardCatalogConnection.Catalog);

                    sqlCmd.CommandText = String.Format("CREATE USER {0} FOR LOGIN {0};", _shardUserName);

                    sqlCmd.ExecuteNonQueryWithRetry();
                }
                catch (Exception) //TODO: Catch & ignore if login already exists to address race condition
                {
                    throw;
                }
            }
        }

        private void DoAlterDbSize(Int32 azureDbSizeGb)
        {
            var cmdText = String.Format("ALTER DATABASE [{0}] MODIFY (MAXSIZE = {1}GB);",
                _shardCatalogConnection.Catalog, azureDbSizeGb);
            ExecuteCommandOnMaster(cmdText);
        }

        private void DoAlterDbSize(Int32 initialSizeMb, Int32 maxSizeMb)
        {
            //This will fail if we are updating a database and not actually changing its size.. As such we will let it fali and just go on as if it "worked"
            //TODO: detect the current database size and max size and only run this script if they have gone up.. (you can't shrink a db this way, we will need to look at a different technique for shrinking.
            try
            {
                var cmdText =
                    String.Format("ALTER DATABASE [{0}] MODIFY FILE (NAME = {0}, SIZE = {1}MB, MAXSIZE = {2}MB);",
                        _shardCatalogConnection.Catalog, initialSizeMb, maxSizeMb);

                ExecuteCommandOnMaster(cmdText);

                if ((Int32) (initialSizeMb*0.1) > 1) //Initial Log size by dacfx is 1MB, command will fail if not larger
                {
                    cmdText =
                        String.Format(
                            "ALTER DATABASE [{0}] MODIFY FILE (NAME = {0}_log, SIZE = {1}MB, MAXSIZE = {2}MB);",
                            _shardCatalogConnection.Catalog, (Int32) (initialSizeMb*0.1), maxSizeMb);

                    ExecuteCommandOnMaster(cmdText);
                }
            }
            catch
            {
                // todo: add Logging
            }
        }

        private void ExecuteCommandOnMaster(String cmdText)
        {
            using (var conn = GetReliableConnection(_masterCatalogConnection.ConnectionString))
            {
                conn.Open();

                try
                {
                    var sqlCmd = conn.CreateCommand();

                    sqlCmd.CommandText = cmdText;

                    sqlCmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // todo: add Logging
                    throw;
                }
            }
        }

        private IEnumerable<string> GetSpNames(String connectionString)
        {
            var spNames = new List<String>();

            using (var conn = GetReliableConnection(connectionString))
            {
                conn.Open();

                try
                {
                    var sqlCmd = conn.CreateCommand();
                    sqlCmd.CommandText =
                        "select p.name as spname, s.name as sname from sys.procedures as p "
                        + "INNER JOIN sys.schemas as s  on p.schema_id = s.schema_id";

                    var reader = sqlCmd.ExecuteReaderWithRetry();

                    while (reader.Read())
                    {
                        var sname = reader["sname"].ToString();
                        var spname = reader["spname"].ToString();

                        spNames.Add(string.Format("{0}.{1}", sname, spname));
                    }
                }
                catch (Exception)
                {
                    // todo: add Logging
                    throw;
                }

                return spNames;
            }
        }

        private void PostPublishDacPacActions(bool isNewDb)
        {
            if (isNewDb)
            {
                if (!IsAzureDb)
                    SetRecoveryModeToSimple();

                AlterDbSize();
                CreateShardUser();
            }

            ApplyPermissionsToShardUser();
        }

        private void ReceiveDacServiceMessage(object sender, DacMessageEventArgs e)
        {
            //TODO: This is useless and going nowhere
            //Logger.WriteToLogText("Message Type:{0} Prefix:{1} Number:{2} Message:{3}", 
            //    e.Message.MessageType, e.Message.Prefix, e.Message.Number, e.Message.Message);
        }

        private void ReceiveDacServiceProgressEvent(object sender, DacProgressEventArgs e)
        {
            //TODO: This is useless and going nowhere
            //Logger.WriteToLogText("Progress Event:{0} Progress Status:{1}", e.Message, e.Status);
        }

        private void SetRecoveryModeToSimple()
        {
            using (var conn = GetReliableConnection(_masterCatalogConnection.ConnectionString))
            {
                conn.Open();

                try
                {
                    var sqlCmd = conn.CreateCommand();

                    sqlCmd.CommandText =
                        String.Format("ALTER DATABASE [{0}] SET RECOVERY SIMPLE;", _shardCatalogConnection.Catalog);

                    sqlCmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // todo: add Logging
                    throw;
                }
            }
        }

        private Boolean ShardedDatabaseExists()
        {
            using (var connection = GetReliableConnection(_masterCatalogConnection.ConnectionString))
            {
                connection.Open();

                try
                {
                    var command = connection.CreateCommand();

                    // this db existance test works in both in SQL and SQL Azure'
                    command.CommandText =
                        String.Format(
                            "IF EXISTS(SELECT * FROM sys.sysdatabases where name='{0}') (SELECT CAST(1 AS bit)) ELSE (SELECT CAST(0 AS bit))",
                            _shardCatalogConnection.Catalog);

                    return (Boolean) command.ExecuteScalar();
                }
                catch (Exception)
                {
                    // todo: add Logging
                    throw;
                }
            }
        }

        #endregion

        #region nested type: DacPacPublisherParams

        public class DacPacPublisherParams
        {
            #region fields

            private readonly ShardBase _shard;
            private readonly string _shardAdminPassword;
            private readonly string _shardAdminUserName;
            private readonly string _shardUserName;
            private readonly string _shardUserPassword;
            private readonly ShardSetConfig _shardSetConfig;

            #endregion

            #region properties

            public ShardBase Shard
            {
                get { return _shard; }
            }

            public string ShardAdminPassword
            {
                get { return _shardAdminPassword; }
            }

            public string ShardAdminUserName
            {
                get { return _shardAdminUserName; }
            }

            public string ShardUserName
            {
                get { return _shardUserName; }
            }

            public string ShardUserPassword
            {
                get { return _shardUserPassword; }
            }

            public ShardSetConfig ShardSetConfig
            {
                get { return _shardSetConfig; }
            }

            #endregion

            #region constructors

            public DacPacPublisherParams(ShardBase shard, ShardSetConfig shardSetConfig, string shardUserName,
                string shardUserPassword, string shardAdminUserName, string shardAdminPassword)
            {
                _shard = shard;
                _shardSetConfig = shardSetConfig;
                _shardUserName = shardUserName;
                _shardUserPassword = shardUserPassword;
                _shardAdminUserName = shardAdminUserName;
                _shardAdminPassword = shardAdminPassword;
            }

            #endregion
        }

        #endregion
    }
}