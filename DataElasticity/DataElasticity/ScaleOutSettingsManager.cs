using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

namespace Microsoft.AzureCat.Patterns.DataElasticity
{
    internal class ScaleOutSettingsManager : UnityBasedManager<ScaleOutSettingsManager>
    {
        #region fields

        private readonly ISettingsRepository _settingsRepository;

        #endregion

        #region constructors

        public ScaleOutSettingsManager(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public Settings AddSettings(Settings settings)
        {
            return _settingsRepository.SaveSettings(settings);
        }
        public Settings GetSettings()
        {
            return _settingsRepository.GetSettings();
        }

        #endregion
    }
}