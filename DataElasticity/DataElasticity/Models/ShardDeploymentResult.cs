#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    internal class ShardDeploymentResult
    {
        #region properties

        public Exception Exception { get; set; }
        public ShardBase Shard { get; set; }

        #endregion
    }
}