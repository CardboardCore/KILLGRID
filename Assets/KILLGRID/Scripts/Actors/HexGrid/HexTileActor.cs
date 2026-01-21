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

        // TODO: Probably need occupied state for multiple placeable types
        [SyncVar] private bool isOccupied;
        [SyncVar(hook = nameof(OnOwnerPlayerIndexChanged))] private int ownerPlayerIndex = -1;

        private float initialFloatY;

        private PlaceableActor placedActor;

        public bool IsOccupied => isOccupied;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position + Vector3.up * config.FloatingHeight, 0.1f);
        }

        protected override void OnInjected()
        {
            // interactableComponent.HoverEnterEvent += OnHoverEnter;
            // interactableComponent.HoverExitEvent += OnHoverExit;
            //
            // interactableComponent.SelectEvent += OnSelect;

            initialFloatY = transform.position.y;

            UpdateMaterial();
        }

        protected override void OnReleased()
        {
            // interactableComponent.HoverEnterEvent -= OnHoverEnter;
            // interactableComponent.HoverExitEvent -= OnHoverExit;
            //
            // interactableComponent.SelectEvent -= OnSelect;
        }

        [Command(requiresAuthority = false)]
        private void Cmd_PlaceActor(PlaceableActor placeableActor)
        {
            placedActor = placeableActor;
            placedActor.SetOccupyingTile(this);

            isOccupied = true;
        }

        [Client]
        private void OnOwnerPlayerIndexChanged(int oldOwnerPlayerIndex, int newOwnerPlayerIndex)
        {
            // Swap material
            UpdateMaterial();

            // Begin floating (or stop floating)
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            float targetFloatY = ownerPlayerIndex == -1 ? initialFloatY : initialFloatY + config.FloatingHeight;

            transform.DOMoveY(targetFloatY, config.FloatingDuration).OnUpdate(() => {
                if (placedActor)
                {
                    // TODO: Disable placeable actor's networktransform after it's placed on the tile
                    placedActor.transform.position = transform.position;
                }
            });
        }

        [Client]
        private void UpdateMaterial()
        {
            Material newMaterial = config.GetMaterialForPlayerIndex(ownerPlayerIndex);
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = newMaterial;
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

        [Server]
        public void SetOwner(int playerIndex)
        {
            ownerPlayerIndex = playerIndex;
        }

        [Server]
        public void OnTurnStart()
        {
            if (placedActor)
            {
                placedActor.OnTurnStart();
            }
        }

        public (int x, int y) GetGridPosition()
        {

        }
    }
}
