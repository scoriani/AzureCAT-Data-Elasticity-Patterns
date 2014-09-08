#region usings

using System;
using Microsoft.Practices.Unity;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity
{
    /// <summary>
    /// Unity base class that helps with the Unity class to perform Service Location. 
    /// </summary>
    /// <typeparam name="T">The utility class we want to make easier to grab an instance of.</typeparam>
    internal abstract class UnityBasedManager<T>
    {
        #region fields

        protected static IUnityContainer _container;
        private static Object _staticLock = new object();

        #endregion

        #region constructors

        public UnityBasedManager()
        {
            if (_container == null)
                LoadUnityContainer();
        }

        #endregion

        #region methods

        public static T GetManager()
        {
            LoadUnityContainer();
            return _container.Resolve<T>();
        }

        private static void LoadUnityContainer()
        {
            _container = UnityHelper.Container;
        }

        #endregion
    }
}