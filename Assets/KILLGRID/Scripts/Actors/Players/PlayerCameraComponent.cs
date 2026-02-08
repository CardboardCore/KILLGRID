using System;
using Attic.Cameras.Transitions;
using Attic.Cameras.VirtualCameras;
using Attic.DI;
using Attic.Mirror.Actors.Components;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Players
{
    [Serializable]
    public class PlayerCameraStepData
    {
        [SerializeField] private string cameraId;

        public string CameraId => cameraId;
    }

    [Serializable]
    public class PlayerCameraStepConfig
    {
        [SerializeField] private PlayerCameraStepData[] steps;

        public PlayerCameraStepData[] Steps => steps;
    }

    public class PlayerCameraComponent : ActorComponent
    {
        [Inject] private VirtualCameraManager virtualCameraManager;

        [Header("References")]
        [SerializeField] private PlayerInputComponent playerInputComponent;

        [Header("Settings")]
        [SerializeField] private PlayerCameraStepConfig cameraStepConfig;

        private int currentStepIndex = 0;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            playerInputComponent.CameraStepForwardEvent += OnCameraStepForward;
            playerInputComponent.CameraStepBackwardEvent += OnCameraStepBackward;

            SetCameraStep(currentStepIndex, true);
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                playerInputComponent.CameraStepForwardEvent -= OnCameraStepForward;
                playerInputComponent.CameraStepBackwardEvent -= OnCameraStepBackward;
            }

            base.OnReleased();
        }

        private void OnCameraStepForward()
        {
            currentStepIndex++;

            if (currentStepIndex >= cameraStepConfig.Steps.Length)
            {
                currentStepIndex = cameraStepConfig.Steps.Length - 1;
            }

            SetCameraStep(currentStepIndex);
        }

        private void OnCameraStepBackward()
        {
            currentStepIndex--;

            if (currentStepIndex < 0)
            {
                currentStepIndex = 0;
            }

            SetCameraStep(currentStepIndex);
        }

        private void SetCameraStep(int stepIndex, bool instant = false)
        {
            stepIndex = Mathf.Clamp(stepIndex, 0, cameraStepConfig.Steps.Length - 1);

            PlayerCameraStepData stepData = cameraStepConfig.Steps[stepIndex];
            virtualCameraManager.DoTransition(stepData.CameraId, instant ? TransitionOptions.Instant : TransitionOptions.None, 0.2f);
        }

        [Client]
        public void ResetCameraStep()
        {
            currentStepIndex = 0;
            SetCameraStep(currentStepIndex);
        }

        [Client]
        public void SetMaxStep()
        {
            currentStepIndex = cameraStepConfig.Steps.Length - 1;
            SetCameraStep(currentStepIndex);
        }
    }
}
