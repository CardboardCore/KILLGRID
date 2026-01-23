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
        public event Action SelectEvent;
        public event Action CameraStepForwardEvent;
        public event Action CameraStepBackwardEvent;

        public event Action CheatIncrementGeneratorEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            inputManager.Player.MousePosition.performed += OnMousePositionPerformed;
            inputManager.Player.Select.performed += OnSelectPerformed;
            inputManager.Player.CameraStepForward.performed += OnCameraStepForwardPerformed;
            inputManager.Player.CameraStepBackward.performed += OnCameraStepBackwardPerformed;

            inputManager.Cheats.IncrementGenerator.performed += OnCheatIncrementGeneratorPerformed;
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                inputManager.Player.MousePosition.performed -= OnMousePositionPerformed;
                inputManager.Player.Select.performed -= OnSelectPerformed;
                inputManager.Player.CameraStepForward.performed -= OnCameraStepForwardPerformed;
                inputManager.Player.CameraStepBackward.performed -= OnCameraStepBackwardPerformed;

                inputManager.Cheats.IncrementGenerator.performed -= OnCheatIncrementGeneratorPerformed;
            }

            base.OnReleased();
        }

        private void OnMousePositionPerformed(InputAction.CallbackContext obj)
        {
            Vector2 mousePosition = obj.ReadValue<Vector2>();
            MousePositionEvent?.Invoke(mousePosition);
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            SelectEvent?.Invoke();
        }

        private void OnCameraStepForwardPerformed(InputAction.CallbackContext context)
        {
            CameraStepForwardEvent?.Invoke();
        }

        private void OnCameraStepBackwardPerformed(InputAction.CallbackContext context)
        {
            CameraStepBackwardEvent?.Invoke();
        }

        private void OnCheatIncrementGeneratorPerformed(InputAction.CallbackContext obj)
        {
            CheatIncrementGeneratorEvent?.Invoke();
        }
    }
}
