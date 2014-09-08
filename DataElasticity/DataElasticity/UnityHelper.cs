#region usings

using System;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity
{
    internal class UnityHelper
    {
        #region properties

        public static UnityContainer Container
        {
            get { return GetContainer(); }
        }

        #endregion

        #region methods

        private static UnityContainer GetContainer()
        {
            var appDomain = AppDomain.CurrentDomain;
            var container = (UnityContainer) appDomain.GetData("UnityContainer");

            if (container == null)
            {
                var singleton = Singleton.Instance;
                container = (UnityContainer) appDomain.GetData("UnityContainer");
            }

            return container;
        }

        #endregion

        #region nested type: Singleton

        private sealed class Singleton
        {
            #region properties

            public static Singleton Instance
            {
                get { return InnerSingleton.instance; }
            }

            #endregion

            #region constructors

            private Singleton()
            {
                var appDomain = AppDomain.CurrentDomain;
                var container = new UnityContainer();

                var section = (UnityConfigurationSection) ConfigurationManager.GetSection("unity");
                section.Configure(container);

                appDomain.SetData("UnityContainer", container);
            }

            #endregion

            #region nested type: InnerSingleton

            private class InnerSingleton
            {
                // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit

                #region fields

                internal static readonly Singleton instance = new Singleton();

                #endregion

                #region constructors

                static InnerSingleton()
                {
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}