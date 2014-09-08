#region usings

using System;
using System.Net;
using System.Threading;
using Common.Logging;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;

#endregion

namespace Microsoft.AzureCat.Patterns.Demo.AdventureWorks.WorkerRole
{
    /// <summary>
    /// Class WorkerRole implements a worker role that processes queue requests to perform 
    /// data elasticity functions.
    /// </summary>
    public class WorkerRole : RoleEntryPoint
    {
        #region constants

        private const int _workerRoleSleepTimeInMilliseconds = _workerRoleSleepTimeInSeconds*1000;
        private const int _workerRoleSleepTimeInSeconds = 10;

        #endregion

        #region fields

        private static Lazy<ILog> _log;
        private Guid _uniqueProcessID;

        #endregion

        #region properties

        private static ILog Log
        {
            get { return _log.Value; }
        }

        #endregion

        #region methods

        /// <summary>
        /// Called once when the worker role is started in Azure.
        /// </summary>
        /// <returns><c>true</c> if the role should continue to run, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// If the OnStart method returns false, the role instance is immediately stopped. If the method returns true, 
        /// Windows Azure starts the role by calling the Run method.
        /// </para>
        /// </remarks>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            using (var config = new SystemConfigurationSource())
            {
                var settings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(config);

                // Initialize the RetryPolicyFactory with a RetryManager built from the 
                // settings in the configuration file.
                RetryPolicyFactory.SetRetryManager(settings.BuildRetryManager());
            }

            // set up a logger
            _log = new Lazy<ILog>(() => LogManager.GetLogger("default"));

            // create a unique process id for this worker.  This is passed to the CopyTenant method on the 
            // and table driver to allow for process specific functionality 
            _uniqueProcessID = Guid.NewGuid();

            return base.OnStart();
        }

        /// <summary>
        /// Override to run the local queue polling.
        /// </summary>
        public override void Run()
        {
            Log.Debug("Azure.Demo entry point called");

            CheckQueues();
            while (true)
            {
                CheckQueues();
            }
        }

        private void CheckQueues()
        {
            // check the queues every so often for a task
            try
            {
                Thread.Sleep(_workerRoleSleepTimeInMilliseconds);

                Log.Info("Checking worker role queue");
                ShardSetActionQueue.GetQueue().CheckAndProcessQueue(_uniqueProcessID);
            }
            catch (Exception ex)
            {
                Log.Error("Exception processing the work role queue", ex);
            }
        }

        #endregion
    }
}