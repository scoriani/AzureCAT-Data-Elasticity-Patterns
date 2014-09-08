#region usings

using System.Collections.Generic;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    public class Server
    {
        #region properties

        public int AvailableShards { get; set; }
        public string Location { get; set; }
        public int MaxShardsAllowed { get; set; }
        public int ServerID { get; set; }
        public string ServerInstanceName { get; set; }

        #endregion

        #region constructors

        public Server()
        {
            ServerID = -1;
        }

        #endregion

        #region methods

        public static IList<Server> GetServers()
        {
            return ScaleOutConfigManager.GetManager().GetServers();
        }

        public static Server Load(string serverName)
        {
            return ScaleOutConfigManager.GetManager().GetServerByName(serverName);
        }

        public static Server Load(int serverID)
        {
            return ScaleOutConfigManager.GetManager().GetServer(serverID);
        }

        public Server Save()
        {
            if (ServerID == -1)
            {
                return ScaleOutConfigManager.GetManager().AddServer(this);
            }
            return ScaleOutConfigManager.GetManager().ModifyServer(this);
        }

        #endregion
    }
}