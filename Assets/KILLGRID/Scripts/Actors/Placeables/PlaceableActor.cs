using System;
using Attic.Mirror.Actors;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Gameplay.Placeables;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{
    [Serializable]
    public class PlaceableActorConfig
    {
        [SerializeField] private PlaceableType placeableType;
        [SerializeField] private GameObject greenHoloView;
        [SerializeField] private GameObject redHoloView;
        [SerializeField] private GameObject normalView;

        public PlaceableType PlaceableType => placeableType;
        public GameObject GreenHoloView => greenHoloView;
        public GameObject RedHoloView => redHoloView;
        public GameObject NormalView => normalView;
    }

    public class PlaceableActor : Actor
    {
        [SerializeField] private PlaceableActorConfig config;

        private bool isSpawned;
        private event Action spawnEvent;

        private HexTileActor occupyingTile;

        private PlaceableActorComponent[] placeableActorComponents;

        /// <summary>
        /// Server only. The tile this placeable is occupying.
        /// </summary>
        public HexTileActor OccupyingTile => occupyingTile;

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
        public void OnTurnStart()
        {
            // TODO: Also here consider a delay between each component's turn start for sequential animations
            foreach (PlaceableActorComponent placeableActorComponent in placeableActorComponents)
            {
                placeableActorComponent.OnTurnStart();
            }
        }

        [Server]
        public void SetOccupyingTile(HexTileActor hexTileActor)
        {
            occupyingTile = hexTileActor;
        }
    }
}
