using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using ConnectionState = DarkRift.ConnectionState;

namespace SteamworksP2P
{
    public class SteamNetworkServerConnection : NetworkServerConnection
    {
        private Connection connection;
        private ConnectionInfo connectionInfo;

        public SteamNetworkServerConnection(Connection connection, ConnectionInfo info)
        {
            this.connection = connection;
            this.connectionInfo = info;
        }

        public override void StartListening()
        {

        }

        public override unsafe bool SendMessageReliable(MessageBuffer message)
        {
            fixed (byte* ptr = message.Buffer)
            {
                Result result = connection.SendMessage((IntPtr)ptr, message.Buffer.Length, SendType.Reliable);

                return result == Result.OK;
            }
        }

        public override unsafe bool SendMessageUnreliable(MessageBuffer message)
        {
            fixed (byte* ptr = message.Buffer)
            {
                Result result = connection.SendMessage((IntPtr)ptr, message.Buffer.Length, SendType.Unreliable);

                return result == Result.OK;
            }
        }

        public override bool Disconnect()
        {
            return connection.Close();
        }

        public void HandleMessage(NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            MessageBuffer buffer = MessageBuffer.Create(size);
            Marshal.Copy(data, buffer.Buffer, 0, size);
            buffer.Count = size;
            HandleMessageReceived(buffer, SendMode.Reliable);
        }

        public void UpdateInfo(ConnectionInfo info)
        {
            connectionInfo = info;
        }

        public void Disconnect(ConnectionInfo info)
        {
            connectionInfo = info;
            HandleDisconnection();
        }

        public override ConnectionState ConnectionState
        {
            get
            {
                switch (connectionInfo.State)
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

        public override IEnumerable<IPEndPoint> RemoteEndPoints => new[] {new IPEndPoint(0, 0)};

        public override IPEndPoint GetRemoteEndPoint(string name)
        {
            return new IPEndPoint(0, 0);
        }
    }
}
