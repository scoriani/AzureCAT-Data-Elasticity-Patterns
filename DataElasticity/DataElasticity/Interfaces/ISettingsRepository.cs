#region usings

using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Interfaces
{
    /// <summary>
    /// Interface to the repository that stores settings data.
    /// </summary>
    public interface ISettingsRepository
    {
        #region methods

        /// <summary>
        /// Get the current global settings for sharding.
        /// </summary>
        /// <returns>Current Settings instance</returns>
        Settings GetSettings();

        /// <summary>
        /// Save the current global settings for sharding.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>Current Settings instance</returns>
        Settings SaveSettings(Settings settings);

        #endregion
    }
}