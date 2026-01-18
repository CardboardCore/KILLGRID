using System;
using Attic.DI;
using Attic.Mirror.Actors.Components;
using KILLGRID.Input;

namespace KILLGRID.Actors.Players
{
    public class PlayerInputComponent : ActorComponent
    {
        [Inject] private InputManager inputManager;

        public event Action CameraStepForwardEvent;
        public event Action CameraStepBackwardEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            inputManager.Player.CameraStepForward.performed += OnCameraStepForward;
            inputManager.Player.CameraStepBackward.performed += OnCameraStepBackward;
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                inputManager.Player.CameraStepForward.performed -= OnCameraStepForward;
                inputManager.Player.CameraStepBackward.performed -= OnCameraStepBackward;
            }

            base.OnReleased();
        }

        private void OnCameraStepForward(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            CameraStepForwardEvent?.Invoke();
        }

        private void OnCameraStepBackward(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            CameraStepBackwardEvent?.Invoke();
        }
    }
}
