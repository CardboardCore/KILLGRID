using System;
using Attic.DI;
using Attic.Utilities;
using Mirror;
using PrisonBreak.Players;

namespace KILLGRID.Networking.Mirror
{
    [Injectable]
    public class GameNetworkManager : NetworkManager
    {
        [Inject] private PlayerManager playerManager;

        public event Action ServerSceneChangedEvent;
        public event Action<NetworkConnectionToClient> ServerConnectedEvent;
        public event Action<NetworkConnectionToClient> ServerDisconnectedEvent;
        public event Action ClientSceneChangedEvent;
        public event Action ClientDisconnectedEvent;
        public event Action<bool> ClientConnectedToServerEvent;

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);

            ServerSceneChangedEvent?.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);

            // Spawn the player if this is not the host connection.
            if (conn.connectionId != NetworkServer.connections[0].connectionId)
            {
                OnServerAddPlayer(conn);
            }

            ServerConnectedEvent?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);

            Injector.Inject(this);

            playerManager.UnassignPlayer(conn);

            Injector.Release(this);

            ServerDisconnectedEvent?.Invoke(conn);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Injector.Inject(this);

            playerManager.AssignPlayerCharacter(conn, GetStartPosition());

            Injector.Release(this);
        }

        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();

            ClientSceneChangedEvent?.Invoke();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            Log.Write("Client connected to server.");

            ClientConnectedToServerEvent?.Invoke(true);
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            ClientDisconnectedEvent?.Invoke();
        }

        public override void OnClientError(TransportError error, string reason)
        {
            base.OnClientError(error, reason);

            Log.Write(reason);

            switch (error)
            {
                case TransportError.Timeout:
                case TransportError.DnsResolve:
                case TransportError.Refused:
                case TransportError.Unexpected:
                    ClientConnectedToServerEvent?.Invoke(false);
                    break;
            }
        }
    }
}
