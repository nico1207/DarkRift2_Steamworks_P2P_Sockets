using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using DarkRift;
using DarkRift.Client;
using Steamworks;
using Steamworks.Data;
using ConnectionState = DarkRift.ConnectionState;

namespace SteamworksP2P
{
    public class SteamNetworkClientConnection : NetworkClientConnection, IConnectionManager
    {
        private SteamId serverId;
        private ConnectionManager connectionManager;

        public SteamNetworkClientConnection(SteamId serverId)
        {
            this.serverId = serverId;
        }

        public override void Connect()
        {
            connectionManager = SteamNetworkingSockets.ConnectRelay<ConnectionManager>(serverId);
            connectionManager.Interface = this;
            ClientManager.Instance.SetConnectionManager(connectionManager);
        }

        public override unsafe bool SendMessageReliable(MessageBuffer message)
        {
            if (connectionManager.Connected)
            {
                fixed (byte* ptr = message.Buffer)
                {
                    Result result = connectionManager.Connection.SendMessage((IntPtr)ptr, message.Buffer.Length, SendType.Reliable);
                    return result == Result.OK;
                }
            }

            return false;
        }

        public override unsafe bool SendMessageUnreliable(MessageBuffer message)
        {
            if (connectionManager.Connected)
            {
                fixed (byte* ptr = message.Buffer)
                {
                    Result result = connectionManager.Connection.SendMessage((IntPtr)ptr, message.Buffer.Length, SendType.Unreliable);
                    return result == Result.OK;
                }
            }

            return false;
        }

        public override bool Disconnect()
        {
            return connectionManager.Connection.Close();
        }

        public override ConnectionState ConnectionState
        {
            get
            {
                switch(connectionManager.ConnectionInfo.State)
                {
                    case Steamworks.ConnectionState.Connected: return ConnectionState.Connected;
                    case Steamworks.ConnectionState.FindingRoute:
                    case Steamworks.ConnectionState.Connecting: return ConnectionState.Connecting;
                    case Steamworks.ConnectionState.ProblemDetectedLocally:
                    case Steamworks.ConnectionState.ClosedByPeer:
                    case Steamworks.ConnectionState.Linger:
                    case Steamworks.ConnectionState.FinWait:
                    case Steamworks.ConnectionState.Dead: return ConnectionState.Disconnected;
                    default: return ConnectionState.Disconnected;
                }
            }
        }

        public override IEnumerable<IPEndPoint> RemoteEndPoints => new[] { new IPEndPoint(0, 0) };

        public override IPEndPoint GetRemoteEndPoint(string name)
        {
            return new IPEndPoint(0, 0);
        }

        public void OnConnecting(ConnectionInfo info)
        {
            
        }

        public void OnConnected(ConnectionInfo info)
        {
            
        }

        public void OnDisconnected(ConnectionInfo info)
        {
            HandleDisconnection();
        }

        public void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            MessageBuffer buffer = MessageBuffer.Create(size);
            Marshal.Copy(data, buffer.Buffer, 0, size);
            buffer.Count = size;
            HandleMessageReceived(buffer, SendMode.Reliable);
        }
    }
}
