using System;
using Attic.Mirror.Actors;
using Attic.Utilities;
using DG.Tweening;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.HexGrid
{
    [Serializable]
    public class HexTileConfig
    {
        [Header("Materials")]
        [SerializeField] private Material neutralMaterial;
        [SerializeField] private Material playerOneMaterial;
        [SerializeField] private Material playerTwoMaterial;

        [Header("Floating Animation Settings")]
        [SerializeField] private float floatingHeight = 0.2f;
        [SerializeField] private float floatingDuration = 1.0f;
        [SerializeField] private Ease floatingEase = Ease.InOutSine;

        public float FloatingHeight => floatingHeight;
        public float FloatingDuration => floatingDuration;
        public Ease FloatingEase => floatingEase;

        public Material GetMaterialForPlayerIndex(int playerIndex)
        {
            return playerIndex switch
            {
                0 => playerOneMaterial,
                1 => playerTwoMaterial,
                -1 => neutralMaterial,
                _ => throw new ArgumentOutOfRangeException(nameof(playerIndex), playerIndex, null)
            };
        }
    }

    public class HexTileActor : Actor
    {
        [Header("References")]
        [SerializeField] private InteractableComponent interactableComponent;

        [Header("Settings")]
        [SerializeField] private HexTileConfig config;

        [SyncVar(hook = nameof(OnOwnerPlayerIndexChanged))] private int ownerPlayerIndex = -1;
        // TODO: Probably need occupied state for multiple placeable types
        [SyncVar] private bool isOccupied;

        [SyncVar] private int gridCoordinatesX;
        [SyncVar] private int gridCoordinatesY;

        private float initialFloatY;

        [SyncVar] private uint placedActorNetId;

        public int OwnerPlayerIndex => ownerPlayerIndex;
        public bool IsOccupied => isOccupied;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position + Vector3.up * config.FloatingHeight, 0.1f);
        }

        protected override void OnInjected()
        {
            initialFloatY = transform.position.y;

            UpdateMaterial();
        }

        [Command(requiresAuthority = false)]
        private void Cmd_PlaceActor(PlaceableActor placeableActor)
        {
            placedActorNetId = placeableActor.netId;
            placeableActor.SetOccupyingTile(this);

            isOccupied = true;
        }

        [Client]
        private void OnOwnerPlayerIndexChanged(int oldOwnerPlayerIndex, int newOwnerPlayerIndex)
        {
            UpdateMaterial();
            UpdatePosition();
        }

        [Client]
        private void UpdatePosition()
        {
            float targetFloatY = ownerPlayerIndex == -1 ? initialFloatY : initialFloatY + config.FloatingHeight;
            transform.DOMoveY(targetFloatY, config.FloatingDuration);
        }

        [Client]
        private void UpdateMaterial()
        {
            Material newMaterial = config.GetMaterialForPlayerIndex(ownerPlayerIndex);
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = newMaterial;
        }

        [Server]
        public void SetCoordinates(int x, int y)
        {
            gridCoordinatesX = x;
            gridCoordinatesY = y;
        }

        [Server]
        public void SetOwner(int playerIndex)
        {
            ownerPlayerIndex = playerIndex;
        }

        [Server]
        public void OnTurnStart()
        {
            if (!NetworkServer.spawned.TryGetValue(placedActorNetId, out NetworkIdentity networkIdentity))
            {
                return;
            }

            PlaceableActor placedActor = networkIdentity.GetComponent<PlaceableActor>();

            if (placedActor)
            {
                placedActor.OnTurnStart();
            }
        }

        [Server]
        public void OnTurnEnd()
        {
            if (!NetworkServer.spawned.TryGetValue(placedActorNetId, out NetworkIdentity networkIdentity))
            {
                return;
            }

            PlaceableActor placedActor = networkIdentity.GetComponent<PlaceableActor>();

            if (placedActor)
            {
                placedActor.OnTurnEnd();
            }
        }

        [Client]
        public void MoveActorToTile(Actor actor)
        {
            if (!actor.isOwned)
            {
                Log.Error($"Trying to move actor {actor.name} that is not owned by this client.");
                return;
            }

            actor.transform.DOMove(transform.position, 0.1f);
        }

        [Client]
        public void RequestPlaceActor(PlaceableActor placeableActor)
        {
            Cmd_PlaceActor(placeableActor);
        }

        public (int x, int y) GetGridPosition()
        {
            return new ValueTuple<int, int>(gridCoordinatesX, gridCoordinatesY);
        }

        [Client]
        public bool TryGetPlacedActor(out PlaceableActor placeableActor)
        {
            placeableActor = null;

            if (!NetworkServer.spawned.TryGetValue(placedActorNetId, out NetworkIdentity networkIdentity))
            {
                return false;
            }

            placeableActor = networkIdentity.GetComponent<PlaceableActor>();

            if (!placeableActor)
            {
                return false;
            }

            return true;
        }
    }
}
