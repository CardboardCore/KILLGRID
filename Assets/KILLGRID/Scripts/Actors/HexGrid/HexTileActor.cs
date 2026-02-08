using System;
using Attic.Mirror.Actors;
using Attic.Utilities;
using DG.Tweening;
using HighlightPlus;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.Placeables.PlaceableActorComponents;
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
        [SerializeField] private Material energizedMaterial;

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

        public Material EnergyMaterial => energizedMaterial;
    }

    public class HexTileActor : Actor
    {
        [Header("References")]
        [SerializeField] private InteractableComponent interactableComponent;
        [SerializeField] private HighlightEffect buildZomeHighlightEffect;
        [SerializeField] private ParticleSystem energizedParticleSystem;

        [Header("Settings")]
        [SerializeField] private HexTileConfig config;

        private float initialFloatY;

        [SyncVar(hook = nameof(OnOwnerPlayerIndexChanged))] private int ownerPlayerIndex = -1;
        [SyncVar] private bool isOccupied;

        [SyncVar] private int gridCoordinatesX;
        [SyncVar] private int gridCoordinatesY;

        [SyncVar] private uint placedActorNetId;
        [SyncVar] private bool isInBuildZone;
        [SyncVar(hook = nameof(OnEnergizedChanged))] private bool isEnergized;

        public int OwnerPlayerIndex => ownerPlayerIndex;
        public bool IsOccupied => isOccupied;
        public bool IsEligibleForPlacement => !isOccupied && isInBuildZone;
        public bool IsEnergized => isEnergized;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * config.FloatingHeight, 0.1f);
        }

        protected override void OnInjected()
        {
            initialFloatY = transform.position.y;

            UpdateMaterial();

            energizedParticleSystem.Stop();
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
        private void OnEnergizedChanged(bool oldIsEnergized, bool newIsEnergized)
        {
            UpdatePosition();
            UpdateEnergizedHighlight();
            UpdateMaterial();
        }

        [Client]
        private void UpdatePosition()
        {
            bool shouldFloat = ownerPlayerIndex != -1 || isEnergized;
            float targetFloatY = shouldFloat ? initialFloatY + config.FloatingHeight : initialFloatY;

            transform.DOMoveY(targetFloatY, config.FloatingDuration).SetEase(config.FloatingEase);
        }

        [Client]
        private void UpdateMaterial()
        {
            Material newMaterial = !isEnergized ? config.GetMaterialForPlayerIndex(ownerPlayerIndex) : config.EnergyMaterial;

            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = newMaterial;
        }

        [Client]
        private void UpdateEnergizedHighlight()
        {
            if (isEnergized)
            {
                energizedParticleSystem.Play();
            }
            else
            {
                energizedParticleSystem.Stop();
            }
        }

        [ClientRpc]
        private void Rpc_ShowBuildZone(bool show)
        {
            buildZomeHighlightEffect.highlighted = show;
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

            if (isEnergized)
            {
                isEnergized = false;
            }
        }

        [Server]
        public void SetIsEnergized(bool energized)
        {
            isEnergized = energized;
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

        [Command(requiresAuthority = false)]
        public void ShowBuildZone(bool inBuildZone)
        {
            isInBuildZone = inBuildZone;
            Rpc_ShowBuildZone(inBuildZone);
        }

        [Client]
        public void MoveActorToTile(Actor actor)
        {
            // TODO: This happens sometimes, and the next tile move will fix it, but should investigate further
            if (!actor.isOwned)
            {
                Log.Warn($"Trying to move actor {actor.name} that is not owned by this client.");
                // return;
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

        public TileCoords GetTileCoords()
        {
            return TileCoords.FromTuple((gridCoordinatesX, gridCoordinatesY));
        }

        [Client]
        public bool TryGetPlacedActor(out PlaceableActor placeableActor)
        {
            placeableActor = null;

            if (!NetworkClient.spawned.TryGetValue(placedActorNetId, out NetworkIdentity networkIdentity))
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
