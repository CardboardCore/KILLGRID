using System;
using Attic.Cameras.VirtualCameras;
using Attic.DI;
using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.Interactables;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Players
{
    public class PlayerInteractComponent : ActorComponent
    {
        [Inject] private VirtualCameraManager virtualCameraManager;

        [SerializeField] private PlayerInputComponent playerInputComponent;

        private InteractableComponent currentHighlightedInteractableComponent;
        private InteractableComponent currentSelectedInteractableComponent;

        public InteractableComponent CurrentHighlightedInteractableComponent => currentHighlightedInteractableComponent;

        public event Action<InteractableComponent> HoverEvent;
        public event Action<InteractableComponent> SelectEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            playerInputComponent.MousePositionEvent += OnMousePosition;
            playerInputComponent.SelectEvent += OnSelect;
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                playerInputComponent.MousePositionEvent -= OnMousePosition;
                playerInputComponent.SelectEvent -= OnSelect;
            }

            base.OnReleased();
        }

        [Client]
        private void OnMousePosition(Vector2 mousePosition)
        {
            // TODO: This only works if it's my turn

            // Cast ray from camera to mouse position
            Ray ray = virtualCameraManager.CameraController.Camera.ScreenPointToRay(mousePosition);

            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                InteractableComponent interactableComponent = hitInfo.collider.GetComponentInParent<InteractableComponent>();

                if (interactableComponent)
                {
                    if (!interactableComponent.CanInteract(Owner as PlayerActor))
                    {
                        return;
                    }

                    if (currentHighlightedInteractableComponent == interactableComponent)
                    {
                        return;
                    }

                    if (currentHighlightedInteractableComponent)
                    {
                        RemoveHighlightedInteractable();
                    }

                    SetHighlightedInteractable(interactableComponent);
                }
                else
                {
                    // Unhighlight previous
                    if (currentHighlightedInteractableComponent)
                    {
                        RemoveHighlightedInteractable();
                    }
                }

                return;
            }

            // Unhighlight previous
            if (currentHighlightedInteractableComponent)
            {
                RemoveHighlightedInteractable();
            }
        }

        [Client]
        private void OnSelect()
        {
            if (!currentHighlightedInteractableComponent)
            {
                if (currentSelectedInteractableComponent)
                {
                    currentSelectedInteractableComponent.UnSelect(true);
                }

                return;
            }

            if (currentHighlightedInteractableComponent == currentSelectedInteractableComponent)
            {
                return;
            }

            if (currentSelectedInteractableComponent)
            {
                currentSelectedInteractableComponent.UnSelect(true);
            }

            currentHighlightedInteractableComponent.Select(true);
            currentSelectedInteractableComponent = currentHighlightedInteractableComponent;

            SelectEvent?.Invoke(currentSelectedInteractableComponent);
        }

        [Client]
        private void SetHighlightedInteractable(InteractableComponent interactableComponent)
        {
            if (currentHighlightedInteractableComponent != interactableComponent)
            {
                // Unhighlight previous
                if (currentHighlightedInteractableComponent)
                {
                    RemoveHighlightedInteractable();
                }

                // Highlight new
                if (interactableComponent)
                {
                    interactableComponent.ShowHighlight(true, true);
                }

                currentHighlightedInteractableComponent = interactableComponent;

                HoverEvent?.Invoke(currentHighlightedInteractableComponent);
            }
        }

        [Client]
        private void RemoveHighlightedInteractable()
        {
            if (currentHighlightedInteractableComponent)
            {
                currentHighlightedInteractableComponent.ShowHighlight(true, false);
                currentHighlightedInteractableComponent = null;

                HoverEvent?.Invoke(null);
            }
        }

        [Client]
        public void UnselectCurrentSelectedInteractable()
        {
            if (currentSelectedInteractableComponent)
            {
                currentSelectedInteractableComponent.UnSelect(true);
                currentSelectedInteractableComponent = null;
            }
        }

        public void UnHighlightCurrentHighlightedInteractable()
        {
            if (currentHighlightedInteractableComponent)
            {
                currentHighlightedInteractableComponent.ShowHighlight(true, false);
                currentHighlightedInteractableComponent = null;
            }
        }
    }
}
