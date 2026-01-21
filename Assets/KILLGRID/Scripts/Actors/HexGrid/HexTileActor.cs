using Attic.Mirror.Actors;
using Attic.Utilities;
using DG.Tweening;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.HexGrid
{
    public class HexTileActor : Actor
    {
        [SerializeField] private InteractableComponent interactableComponent;

        [SyncVar] private bool isOccupied;

        private PlaceableActor placedActor;

        public bool IsOccupied => isOccupied;

        protected override void OnInjected()
        {
            // interactableComponent.HoverEnterEvent += OnHoverEnter;
            // interactableComponent.HoverExitEvent += OnHoverExit;
            //
            // interactableComponent.SelectEvent += OnSelect;
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
            isOccupied = true;
        }

        [Client]
        public void MoveActorToTile(Actor actor)
        {
            if (!actor.isOwned)
            {
                Log.Error($"Trying to move actor {actor.name} that is not owned by this client.");
                return;
            }

            actor.transform.DOMove(transform.position, 0.3f);
        }

        [Client]
        public void RequestPlaceActor(PlaceableActor placeableActor)
        {
            Cmd_PlaceActor(placeableActor);
        }
    }
}
