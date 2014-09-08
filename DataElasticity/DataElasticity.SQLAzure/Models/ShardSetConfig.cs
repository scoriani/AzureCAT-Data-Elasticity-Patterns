//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.AzureCat.Patterns.DataElasticity.SQLAzure.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ShardSetConfig
    {
        public ShardSetConfig()
        {
            this.ShardSetConfigSettings = new HashSet<ShardSetConfigSetting>();
        }
    
        public int ShardSetConfigID { get; set; }
        public int ShardSetID { get; set; }
        public int Version { get; set; }
        public int TargetShardCount { get; set; }
        public int MaxShardCount { get; set; }
        public long MaxShardletsPerShard { get; set; }
        public int MinShardSizeMB { get; set; }
        public int MaxShardSizeMB { get; set; }
        public bool AllowDeployment { get; set; }
        public int ShardMapID { get; set; }
    
        public virtual ShardMap ShardMap { get; set; }
        public virtual ShardSet ShardSet { get; set; }
        public virtual ICollection<ShardSetConfigSetting> ShardSetConfigSettings { get; set; }
    }
}
