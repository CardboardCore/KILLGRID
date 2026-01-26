using System;
using Attic.Mirror.Actors;
using Attic.Utilities;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Gameplay.Placeables;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{
    [Serializable]
    public class PlaceableActorConfig
    {
        [SerializeField] private GameObject greenHoloView;
        [SerializeField] private GameObject redHoloView;
        [SerializeField] private GameObject normalView;

        public GameObject GreenHoloView => greenHoloView;
        public GameObject RedHoloView => redHoloView;
        public GameObject NormalView => normalView;
    }

    public class PlaceableActor : Actor
    {
        [SerializeField] private PlaceableActorConfig config;

        private bool isSpawned;
        private event Action spawnEvent;

        // Networked ID of the tile this placeable is occupying
        [SyncVar(hook = nameof(OnOccupyingTileChanged))] private uint occupyingTileNetId;

        private PlaceableActorComponent[] placeableActorComponents;

        private HexTileActor occupyingTile;

        /// <summary>
        /// The tile this placeable is occupying.
        /// </summary>
        public HexTileActor OccupyingTile => NetworkClient.spawned.TryGetValue(occupyingTileNetId, out NetworkIdentity identity)
            ? identity.GetComponent<HexTileActor>() : null;

        protected override void OnInjected()
        {
            base.OnInjected();

            HideVisuals(false);

            spawnEvent?.Invoke();
            spawnEvent = null;

            isSpawned = true;

            if (isServer)
            {
                placeableActorComponents = GetComponentsInChildren<PlaceableActorComponent>();

                foreach (PlaceableActorComponent placeableActorComponent in placeableActorComponents)
                {
                    placeableActorComponent.Initialize(this);
                }
            }
        }

        private void Update()
        {
            if (!occupyingTile)
            {
                return;
            }

            transform.position = occupyingTile.transform.position;
        }

        [Client]
        private void OnOccupyingTileChanged(uint oldNetId, uint newNetId)
        {
            if (NetworkClient.spawned.TryGetValue(newNetId, out NetworkIdentity identity))
            {
                occupyingTile = identity.GetComponent<HexTileActor>();
            }
            else
            {
                occupyingTile = null;
            }
        }

        [Command(requiresAuthority = false)]
        private void Cmd_HideVisuals() { Rpc_HideVisuals(); }

        [Command(requiresAuthority = false)]
        private void Cmd_ShowAsGreenHologram() { Rpc_ShowAsGreenHologram(); }

        [Command(requiresAuthority = false)]
        private void Cmd_ShowAsRedHologram() { Rpc_ShowAsRedHologram(); }

        [Command(requiresAuthority = false)]
        private void Cmd_ShowAsNormal() { Rpc_ShowAsNormal(); }

        [ClientRpc]
        private void Rpc_HideVisuals() { HideVisuals(false); }

        [ClientRpc]
        private void Rpc_ShowAsGreenHologram() { ShowAsGreenHologram(false); }

        [ClientRpc]
        private void Rpc_ShowAsRedHologram() { ShowAsRedHologram(false); }

        [ClientRpc]
        private void Rpc_ShowAsNormal() { ShowAsNormal(false); }

        [Client]
        public void WhenSpawned(Action callback)
        {
            if (isSpawned)
            {
                callback();
            }
            else
            {
                spawnEvent += callback;
            }
        }

        [Client]
        public void HideVisuals(bool callCommand)
        {
            config.GreenHoloView.SetActive(false);
            config.NormalView.SetActive(false);

            if (callCommand)
            {
                Cmd_HideVisuals();
            }
        }

        [Client]
        public void ShowAsGreenHologram(bool callCommand)
        {
            config.GreenHoloView.SetActive(true);
            config.RedHoloView.SetActive(false);
            config.NormalView.SetActive(false);

            if (callCommand)
            {
                Cmd_ShowAsGreenHologram();
            }
        }

        [Client]
        public void ShowAsRedHologram(bool callCommand)
        {
            config.GreenHoloView.SetActive(false);
            config.RedHoloView.SetActive(true);
            config.NormalView.SetActive(false);

            if (callCommand)
            {
                Cmd_ShowAsRedHologram();
            }
        }

        [Client]
        public void ShowAsNormal(bool callCommand)
        {
            config.GreenHoloView.SetActive(false);
            config.RedHoloView.SetActive(false);
            config.NormalView.SetActive(true);

            if (callCommand)
            {
                Cmd_ShowAsNormal();
            }
        }

        [Server]
        public void SetOccupyingTile(HexTileActor hexTileActor)
        {
            occupyingTileNetId = hexTileActor.netId;

            foreach (PlaceableActorComponent placeableActorComponent in placeableActorComponents)
            {
                placeableActorComponent.OnPlaced();
            }
        }

        [Server]
        public void OnTurnStart()
        {
            // TODO: Also here consider a delay between each component's turn start for sequential animations
            foreach (PlaceableActorComponent placeableActorComponent in placeableActorComponents)
            {
                placeableActorComponent.OnTurnStart();
            }
        }

        [Server]
        public void OnTurnEnd()
        {
            foreach (PlaceableActorComponent placeableActorComponent in placeableActorComponents)
            {
                placeableActorComponent.OnTurnEnd();
            }
        }
    }
}
