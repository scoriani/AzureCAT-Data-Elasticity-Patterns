#region usings

using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Settings
{
    internal class AzureSettingContext : TableServiceContext
    {
        #region constants

        public const string SettingsTable = "settingstable";

        #endregion

        #region properties

        public IQueryable<AzureSetting> Settings
        {
            get { return CreateQuery<AzureSetting>(SettingsTable); }
        }

        #endregion

        #region constructors

        public AzureSettingContext(CloudTableClient client)
            : base(client)
        {
            IgnoreResourceNotFoundException = true;
        }

        #endregion
    }
}