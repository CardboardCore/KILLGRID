using Attic.Cameras.VirtualCameras;
using Attic.DI;
using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.Interactables;
using UnityEngine;

namespace KILLGRID.Actors.Players
{
    public class PlayerInteractableComponent : ActorComponent
    {
        [Inject] private VirtualCameraManager virtualCameraManager;

        [SerializeField] private PlayerInputComponent playerInputComponent;

        private InteractableActor currentHighlightedInteractableActor;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            playerInputComponent.MousePositionEvent += OnMousePositionEvent;
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                playerInputComponent.MousePositionEvent -= OnMousePositionEvent;
            }

            base.OnReleased();
        }

        private void OnMousePositionEvent(Vector2 mousePosition)
        {
            // TODO: This only works if it's my turn

            // Cast ray from camera to mouse position
            Ray ray = virtualCameraManager.CameraController.Camera.ScreenPointToRay(mousePosition);

            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                InteractableActor interactableActor = hitInfo.collider.GetComponentInParent<InteractableActor>();

                if (interactableActor)
                {
                    if (currentHighlightedInteractableActor != interactableActor)
                    {
                        SetHighlightedInteractable(interactableActor);
                    }
                }
                // Unhighlight previous
                else if (currentHighlightedInteractableActor)
                {
                    RemoveHighlightedInteractable();
                }

                return;
            }

            // Unhighlight previous
            if (currentHighlightedInteractableActor)
            {
                RemoveHighlightedInteractable();
            }
        }

        private void SetHighlightedInteractable(InteractableActor interactableActor)
        {
            if (currentHighlightedInteractableActor != interactableActor)
            {
                // Unhighlight previous
                if (currentHighlightedInteractableActor)
                {
                    RemoveHighlightedInteractable();
                }

                // Highlight new
                if (interactableActor)
                {
                    interactableActor.ShowHighlight(true, true);
                }

                currentHighlightedInteractableActor = interactableActor;
            }
        }

        private void RemoveHighlightedInteractable()
        {
            if (currentHighlightedInteractableActor)
            {
                currentHighlightedInteractableActor.ShowHighlight(true, false);
                currentHighlightedInteractableActor = null;
            }
        }
    }
}
