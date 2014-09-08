#region usings

using Microsoft.WindowsAzure.ServiceRuntime;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole
{
    public class WebRole : RoleEntryPoint
    {
        #region methods

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        #endregion
    }
}