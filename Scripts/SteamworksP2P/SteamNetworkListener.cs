using System;
using System.Collections.Generic;
using DarkRift.Server;
using Steamworks;
using Steamworks.Data;

namespace SteamworksP2P
{
    public class SteamNetworkListener : NetworkListener, ISocketManager
    {
        private SocketManager socketManager;
        private Dictionary<uint, SteamNetworkServerConnection> connections;

        public SteamNetworkListener(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
        {
            connections = new Dictionary<uint, SteamNetworkServerConnection>();
        }

        public override Version Version => new Version(0, 0, 1);

        public override void StartListening()
        {
            if (SteamClient.IsValid)
            {
                socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
                socketManager.Interface = this;

                ServerManager.Instance.SetSocketManager(socketManager);
            }
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime,
            int channel)
        {
            if (connections.ContainsKey(connection.Id))
                connections[connection.Id].HandleMessage(identity, data, size, messageNum, recvTime, channel);
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            if (connections.ContainsKey(connection.Id))
                connections[connection.Id].Disconnect(info);

            connections.Remove(connection.Id);
        }

        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            connection.Accept();
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            if (!connections.ContainsKey(connection.Id))
            {
                SteamNetworkServerConnection serverConnection = new SteamNetworkServerConnection(connection, info);
                connections.Add(connection.Id, serverConnection);
                RegisterConnection(serverConnection);
            }
        }
    }
}
