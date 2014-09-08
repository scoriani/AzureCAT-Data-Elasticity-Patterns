#region usings

using System;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure
{
    public class EfShardConnectionBuilder
    {
        #region methods

        public static T MakeContext<T>(string connectionString, string modelName)
        {
            var entConnection =
                string.Format(
                    "metadata=res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl;provider=System.Data.SqlClient;provider connection string=\"{1}\"",
                    modelName,
                    connectionString);
            var context = (T) Activator.CreateInstance(typeof (T), entConnection);
            return context;
        }

        #endregion
    }
}