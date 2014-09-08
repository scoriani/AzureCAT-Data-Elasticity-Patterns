#region usings

using System.Data.Entity;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure.Models
{
    public partial class DataElasticityEntities : DbContext
    {
        #region constructors

        public DataElasticityEntities(string connectionString)
            : base(connectionString)
        {
        }

        #endregion
    }
}
