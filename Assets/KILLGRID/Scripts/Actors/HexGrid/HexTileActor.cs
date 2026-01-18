using Attic.DI;
using Attic.Mirror.Actors;
using KILLGRID.Actors.Interactables;
using KILLGRID.Gameplay.BoardPlacement;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.HexGrid
{
    public class HexTileActor : Actor
    {
        [Inject] private BoardPlaceableManager boardPlaceableManager;

        [SerializeField] private InteractableComponent interactableComponent;

        [SyncVar] private bool isOccupied;

        protected override void OnInjected()
        {
            interactableComponent.HoverEnterEvent += OnHoverEnter;
            interactableComponent.HoverExitEvent += OnHoverExit;

            interactableComponent.SelectEvent += OnSelect;
        }

        protected override void OnReleased()
        {
            interactableComponent.HoverEnterEvent -= OnHoverEnter;
            interactableComponent.HoverExitEvent -= OnHoverExit;

            interactableComponent.SelectEvent -= OnSelect;
        }

        [Client]
        private void OnHoverEnter()
        {
            boardPlaceableManager.Cmd_ShowPlaceableAtTile(this);
        }

        [Client]
        private void OnHoverExit()
        {
            boardPlaceableManager.Cmd_HidePlaceable();
        }

        [Client]
        private void OnSelect()
        {
            if (isOccupied)
            {
                return;
            }

            Cmd_SetOccupied();
        }

        [Command(requiresAuthority = false)]
        private void Cmd_SetOccupied()
        {
            if (boardPlaceableManager.CurrentPlaceableConfig == null)
            {
                return;
            }

            boardPlaceableManager.ClearCachedPlaceableConfig();

            interactableComponent.UnSelect(true);

            isOccupied = true;
        }
    }
}
