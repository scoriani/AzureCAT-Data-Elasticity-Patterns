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
    
    public partial class RangeShard
    {
        public int ShardID { get; set; }
        public int ShardMapID { get; set; }
        public int DatabaseID { get; set; }
        public long RangeLowValue { get; set; }
        public long RangeHighValue { get; set; }
    
        public virtual Database Database { get; set; }
        public virtual ShardMap ShardMap { get; set; }
    }
}