using System;
using System.Collections.Generic;
using Attic.DI;
using Attic.Utilities;
using KILLGRID.Actors.PlayerMonitors;
using Mirror;
using KILLGRID.Actors.Players;
using UnityEngine;

namespace PrisonBreak.Players
{
    public class PlayerEntry
    {
        public int ConnectionId;
        public PlayerActor Player;
        public string Name;
    }

    [Injectable]
    public class PlayerManager : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerActor playerActorOnePrefab;
        [SerializeField] private PlayerActor playerActorTwoPrefab;

        [Header("Monitors")]
        [SerializeField] private PlayerMonitorComponent playerOneMonitor;
        [SerializeField] private PlayerMonitorComponent playerTwoMonitor;

        [Header("Settings")]
        [SerializeField] private bool awaitFullGame;

        private readonly SyncList<PlayerEntry> playerEntries = new SyncList<PlayerEntry>();

        public bool AwaitFullGame => awaitFullGame;
        public int PlayerCount => playerEntries.Count;

        public event Action<PlayerEntry> PlayerAddedEvent;

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

            PlayerMonitorComponent playerMonitor = index == 0 ? playerOneMonitor : playerTwoMonitor;
            entry.Player.SetMonitor(playerMonitor, entry.Name);

            PlayerAddedEvent?.Invoke(entry);
        }

        private void OnPlayerEntryRemoved(int index, PlayerEntry playerEntry)
        {
            // Handle player removal if necessary
        }

        [Server]
        public void AssignPlayerCharacter(NetworkConnectionToClient conn, Transform startPoint)
        {
            Log.Write($"Spawning player for connection ID {conn.connectionId}");

            // TODO: This currently holds up as when the host leaves (player one), player two will get disconnected.
            // Check if player entries [0] exists to decide if this is player one or player two
            PlayerActor prefab = playerEntries.Count == 0 ? playerActorOnePrefab : playerActorTwoPrefab;

            PlayerActor playerActor = Instantiate(prefab, startPoint.position, startPoint.rotation);
            playerActor.name = $"{prefab.name} [connId={conn.connectionId}]";

            NetworkServer.AddPlayerForConnection(conn, playerActor.gameObject);

            PlayerEntry playerEntry = new PlayerEntry
            {
                ConnectionId = conn.connectionId,
                Player = playerActor,
                Name = playerEntries.Count == 0 ? "<Player 1>" : "<Player 2>" // TODO: Replace with actual player names
            };

            playerEntries.Add(playerEntry);

            playerActor.SetPlayerIndex(playerEntries.IndexOf(playerEntry));
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

        [Server]
        public PlayerActor GetPlayer(int index)
        {
            if (playerEntries.Count <= index)
            {
                Log.Error($"Player index {index} is out of range. Total players: {playerEntries.Count}");
                return null;
            }

            return playerEntries[index].Player;
        }

        public List<PlayerActor> GetAllPlayers()
        {
            List<PlayerActor> players = new List<PlayerActor>();

            foreach (PlayerEntry entry in playerEntries)
            {
                players.Add(entry.Player);
            }

            return players;
        }
    }
}
