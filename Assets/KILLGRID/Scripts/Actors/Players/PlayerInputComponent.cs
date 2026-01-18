using System;
using Attic.DI;
using Attic.Mirror.Actors.Components;
using KILLGRID.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KILLGRID.Actors.Players
{
    public class PlayerInputComponent : ActorComponent
    {
        [Inject] private InputManager inputManager;

        public event Action<Vector2> MousePositionEvent;
        public event Action CameraStepForwardEvent;
        public event Action CameraStepBackwardEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            inputManager.Player.MousePosition.performed += OnMousePositionPerformed;
            inputManager.Player.CameraStepForward.performed += OnCameraStepForward;
            inputManager.Player.CameraStepBackward.performed += OnCameraStepBackward;
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                inputManager.Player.MousePosition.performed -= OnMousePositionPerformed;
                inputManager.Player.CameraStepForward.performed -= OnCameraStepForward;
                inputManager.Player.CameraStepBackward.performed -= OnCameraStepBackward;
            }

            base.OnReleased();
        }

        private void OnMousePositionPerformed(InputAction.CallbackContext obj)
        {
            Vector2 mousePosition = obj.ReadValue<Vector2>();
            MousePositionEvent?.Invoke(mousePosition);
        }

        private void OnCameraStepForward(InputAction.CallbackContext context)
        {
            CameraStepForwardEvent?.Invoke();
        }

        private void OnCameraStepBackward(InputAction.CallbackContext context)
        {
            CameraStepBackwardEvent?.Invoke();
        }
    }
}
