using System;
using Attic.DI;
using Attic.Mirror;
using Attic.Utilities;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.Players;
using Mirror;
using UnityEngine;

namespace KILLGRID.Gameplay.Placeables
{
    [Injectable]
    public class PlaceablesFactory : AtticNetworkBehaviour
    {
        [SerializeField] private PlaceablesConfig placeablesConfig;

        public PlaceablesConfig PlaceablesConfig => placeablesConfig;

        public event Action<PlaceableActor> PlaceableSpawnedEvent;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Command(requiresAuthority = false)]
        private void Cmd_Spawn(PlayerActor playerActor, PlaceableType placeableType, HexTileActor hexTileActor)
        {
            if (!placeablesConfig.TryGetPlaceableConfig(placeableType, out PlaceableConfig config))
            {
                Log.Error($"No placeable config found for type {placeableType}");
                return;
            }

            PlaceableActor placeablePrefab = config.PlaceableActorPrefab;
            Vector3 spawnPosition = hexTileActor.transform.position;
            Quaternion spawnRotation = Quaternion.identity;

            PlaceableActor placeableInstance = Instantiate(placeablePrefab, spawnPosition, spawnRotation);
            NetworkServer.Spawn(placeableInstance.gameObject);

            placeableInstance.Initialize(playerActor.PlayerIndex, placeableType);

            Rpc_SpawnedPlaceable(playerActor.netIdentity.connectionToClient, placeableInstance);
        }

        [TargetRpc]
        private void Rpc_SpawnedPlaceable(NetworkConnectionToClient target, PlaceableActor placeableActor)
        {
            PlaceableSpawnedEvent?.Invoke(placeableActor);
        }

        [Client]
        public void RequestSpawn(PlayerActor playerActor, PlaceableType placeableType, HexTileActor hexTileActor)
        {
            Cmd_Spawn(playerActor, placeableType, hexTileActor);
        }
    }
}
