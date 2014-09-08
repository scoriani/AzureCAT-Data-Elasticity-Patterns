using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Client.IntegrationTests
{
    /// <summary>
    /// Class ElasticSqlConnectionTest tests the ElasticSqlConnection class.
    /// </summary>
    /// <remarks>
    /// In order to run these tests:
    /// <list type="number">
    /// <item>The configured Azure storage must be available.</item>
    /// <item>The shard map must be published.</item>
    /// </list>
    /// </remarks>
    [TestClass]
    public class ElasticSqlConnectionTest
    {
        #region methods

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SetUpRetryPolicy();
        }

        [TestMethod]
        public void ShouldConnectToShard()
        {
            //assemble
            var shardingKey = 1.ToString(CultureInfo.InvariantCulture);
            var connection = new ElasticSqlConnection("AWSales", shardingKey);

            using (connection)
            {
                connection.Open();
            }
        }

        private static void SetUpRetryPolicy()
        {
            using (var config = new SystemConfigurationSource())
            {
                var settings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(config);

                // Initialize the RetryPolicyFactory with a RetryManager built from the 
                // settings in the configuration file.
                var buildRetryManager = settings.BuildRetryManager();
                RetryPolicyFactory.SetRetryManager(buildRetryManager);
            }
        }

        #endregion
    }
}