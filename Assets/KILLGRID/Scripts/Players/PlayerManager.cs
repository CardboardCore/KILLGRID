using System.Collections.Generic;
using Attic.DI;
using Attic.Utilities;
using Mirror;
using KILLGRID.Actors.Players;
using UnityEngine;

namespace PrisonBreak.Players
{
    public class PlayerEntry
    {
        public int ConnectionId;
        public PlayerActor Player;
    }

    [Injectable]
    public class PlayerManager : NetworkBehaviour
    {
        [SerializeField] private PlayerActor playerActorPrefab;

        private readonly SyncList<PlayerEntry> playerEntries = new SyncList<PlayerEntry>();

        public override void OnStartServer()
        {
            base.OnStartServer();

            Injector.Inject(this);
        }

        public override void OnStopServer()
        {
            Injector.Release(this);

            base.OnStopServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            playerEntries.OnAdd += OnPlayerEntryAdded;
            playerEntries.OnRemove += OnPlayerEntryRemoved;

            for (int i = 0; i < playerEntries.Count; i++)
            {
                playerEntries.OnAdd.Invoke(i);
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            playerEntries.OnAdd -= OnPlayerEntryAdded;
            playerEntries.OnRemove -= OnPlayerEntryRemoved;
        }

        private void OnPlayerEntryAdded(int index)
        {
            PlayerEntry entry = playerEntries[index];
            // entry.Player.Initialize();
        }

        private void OnPlayerEntryRemoved(int index, PlayerEntry playerEntry)
        {
            // Handle player removal if necessary
        }

        [Server]
        public void AssignPlayerCharacter(NetworkConnectionToClient conn, Transform startPoint)
        {
            Log.Write($"Spawning player for connection ID {conn.connectionId}");

            PlayerActor playerActor = Instantiate(playerActorPrefab, startPoint.position, startPoint.rotation);
            playerActor.name = $"{playerActorPrefab.name} [connId={conn.connectionId}]";

            NetworkServer.AddPlayerForConnection(conn, playerActor.gameObject);

            PlayerEntry playerEntry = new PlayerEntry
            {
                ConnectionId = conn.connectionId,
                Player = playerActor
            };

            playerEntries.Add(playerEntry);
        }

        [Server]
        public void UnassignPlayer(NetworkConnectionToClient conn)
        {
            for (int i = 0; i < playerEntries.Count; i++)
            {
                if (playerEntries[i].ConnectionId == conn.connectionId)
                {
                    playerEntries.RemoveAt(i);

                    return;
                }
            }
        }
    }
}
