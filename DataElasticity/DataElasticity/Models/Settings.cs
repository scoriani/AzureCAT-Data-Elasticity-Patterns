namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class Settings
    {
        #region properties

        public string AdminPassword { get; set; }
        public string AdminUser { get; set; }
        public string ShardPassword { get; set; }
        public string ShardPrefix { get; set; }
        public string ShardUser { get; set; }

        #endregion

        #region methods

        public static Settings Load()
        {
            return ScaleOutSettingsManager.GetManager().GetSettings();
        }

        public Settings Save()
        {
            return ScaleOutSettingsManager.GetManager().AddSettings(this);
        }

        #endregion
    }
}