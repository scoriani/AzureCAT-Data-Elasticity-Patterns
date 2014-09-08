namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    /// <summary>
    /// Class ShardSetConfigSetting is the entity for a single custom setting, 
    /// collected in a <see cref="ShardSetConfigSetting"/>.
    /// </summary>
    public class ShardSetConfigSetting
    {
        #region properties

        /// <summary>
        /// Gets or sets the setting key.
        /// </summary>
        /// <value>The setting key.</value>
        public string SettingKey { get; set; }

        /// <summary>
        /// Gets or sets the setting value.
        /// </summary>
        /// <value>The setting value.</value>
        public string SettingValue { get; set; }

        /// <summary>
        /// Gets or sets the shard set configuration identifier.
        /// </summary>
        /// <value>The shard set configuration identifier.</value>
        public int ShardSetConfigID { get; set; }

        /// <summary>
        /// Gets or sets the shard set configuration setting identifier.
        /// </summary>
        /// <value>The shard set configuration setting identifier.</value>
        public int ShardSetConfigSettingID { get; set; }

        /// <summary>
        /// Gets or sets the shard set configuration.
        /// </summary>
        /// <value>The shard set configuration.</value>
        public ShardSetConfig ShardSetConfig { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardSetConfigSetting"/> class.
        /// </summary>
        public ShardSetConfigSetting()
        {
            ShardSetConfigSettingID = -1;
            ShardSetConfigID = -1;
        }

        #endregion
    }
}