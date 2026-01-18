using System;
using DG.Tweening;
using UnityEngine;

namespace Attic.Cameras.Transitions
{
     public class Transition
    {
        private enum TransitionState
        {
            Idle,
            Active,
            Finished
        }

        private readonly CameraController cameraController;
        private readonly Transform from;
        private readonly Transform to;
        private readonly TransitionConfig transitionConfig;

        // TODO: Add back support for transitioning fov and clipping planes
        // private readonly float startFOV;
        // private readonly float startNearClipPlane;
        // private readonly float startFarClipPlane;

        private TransitionState transitionState;

        private Vector3 positionVelocity;

        private float currentDuration;

        private float fovVelocity;

        private float nearClipVelocity;
        private float farClipVelocity;

        private Sequence sequence;

        internal event Action<Transition> OnFinishedEvent;

        internal Transition(CameraController controller, TransitionConfig config)
        {
            cameraController = controller;
            transitionConfig = config;

            from = transitionConfig.From;
            to = transitionConfig.ToCamera.transform;

            transitionState = TransitionState.Idle;

            // TODO: Add back support for transitioning fov and clipping planes
            // startFOV = transitionConfig.FromIsOrthographic ? 1f : transitionConfig.FromFOV;
            //
            // startNearClipPlane = transitionConfig.FromNearClipPlane;
            //
            // startFarClipPlane = transitionConfig.FromFarClipPlane;
        }

        internal void Start()
        {
            if (transitionState == TransitionState.Active)
            {
                return;
            }

            if (transitionConfig.IsInstant)
            {
                // TODO: Add back support for transitioning fov and clipping planes
                // cameraController.Camera.nearClipPlane = config.ToCamera.NearClipPlane;
                // cameraController.Camera.farClipPlane = config.ToCamera.FarClipPlane;
                //
                // cameraController.Camera.fieldOfView = config.ToCamera.FOV;

                cameraController.transform.position = to.position;
                cameraController.transform.rotation = to.rotation;

                transitionState = TransitionState.Finished;
                OnFinishedEvent?.Invoke(this);
            }
            else
            {
                sequence?.Kill();

                sequence = DOTween.Sequence();

                sequence.Append(cameraController.transform.DOMove(to.position, transitionConfig.TransitionTime).SetEase(Ease.InOutQuad));
                sequence.Join(cameraController.transform.DORotate(to.rotation.eulerAngles, transitionConfig.TransitionTime).SetEase(Ease.InOutQuad));

                sequence.OnComplete(() => {
                    transitionState = TransitionState.Finished;
                    OnFinishedEvent?.Invoke(this);
                });

                transitionState = TransitionState.Active;
            }
        }

        internal void Kill(bool fireFinishedEvent = true)
        {
            transitionState = TransitionState.Finished;

            sequence?.Kill();

            if (fireFinishedEvent)
            {
                OnFinishedEvent?.Invoke(this);
            }
        }
    }
}
