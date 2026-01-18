using System;
using Attic.Cameras.VirtualCameras;
using UnityEngine;

namespace Attic.Cameras.Transitions
{
    public class TransitionManager : MonoBehaviour
    {
        private Transition activeTransition;
        private VirtualCamera toVirtualCamera;
        private Transform interruptHelperTransform;

        public event Action<VirtualCamera> OnTransitionFinishedEvent;

        private void CreateTransition(CameraController cameraController, TransitionConfig transitionConfig)
        {
            if (activeTransition != null)
            {
                if (transitionConfig.TransitionOptions.HasFlag(TransitionOptions.NotInterruptable))
                {
                    Debug.LogError("Transition isn't finished yet, please wait before trying to create a new transition");
                    return;
                }

                KillTransition();
                DoTransitionFromCurrentCameraValues(cameraController, transitionConfig);

                return;
            }

            activeTransition = new Transition(cameraController, transitionConfig);
            activeTransition.OnFinishedEvent += OnTransitionFinished;
        }

        private void StartTransition()
        {
            if (activeTransition == null)
            {
                Debug.LogError(
                    "Trying to start a transition, but no new transition was created yet!");

                return;
            }

            activeTransition.Start();
        }

        private void KillTransition(bool fireFinishedEvent = false)
        {
            if (activeTransition == null)
            {
                Debug.LogError("Trying to kill a transition, but no transition is active!");

                return;
            }

            if (!fireFinishedEvent)
            {
                DestroyTransitionHelper();
                activeTransition.OnFinishedEvent -= OnTransitionFinished;
            }

            activeTransition.Kill(fireFinishedEvent);
            activeTransition = null;
        }

        private void OnTransitionFinished(Transition transition)
        {
            if (activeTransition != transition)
            {
                throw new Exception(
                    "A transition was finished, but this wasn't the current active transition! Please investigate...");
            }

            activeTransition.OnFinishedEvent -= OnTransitionFinished;
            activeTransition = null;

            // Doing this to support chaining
            VirtualCamera virtualCamera = toVirtualCamera;
            toVirtualCamera = null;

            DestroyTransitionHelper();

            OnTransitionFinishedEvent?.Invoke(virtualCamera);
        }

        /// <summary>
        /// Immediately interrupt current active transition and start new transition from current Camera values.
        /// </summary>
        /// <param name="cameraController"></param>
        /// <param name="interruptedTransitionConfig"></param>
        private void DoTransitionFromCurrentCameraValues(CameraController cameraController,
                                                         TransitionConfig
                                                             interruptedTransitionConfig)
        {
            CreateTransitionHelper(cameraController);

            TransitionConfig transitionConfig =
                interruptedTransitionConfig.CreateInterruptedConfig(
                    interruptHelperTransform, cameraController.Camera.orthographic,
                    cameraController.Camera.fieldOfView);

            CreateTransition(cameraController, transitionConfig);
            StartTransition();
        }

        private void CreateTransitionHelper(CameraController cameraController)
        {
            GameObject interruptHelper = new GameObject();
            interruptHelper.name = "TransitionInterruptHelper";
            interruptHelper.transform.position = cameraController.transform.position;
            interruptHelper.transform.rotation = cameraController.transform.rotation;

            interruptHelperTransform = interruptHelper.transform;
        }

        private void DestroyTransitionHelper()
        {
            if (interruptHelperTransform != null)
            {
                Destroy(interruptHelperTransform.gameObject);
            }
        }

        /// <summary>
        /// Immediately create and start a transition.
        /// </summary>
        /// <param name="cameraController"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="options"></param>
        /// <param name="smoothTime"></param>
        internal void DoTransition(CameraController cameraController, VirtualCamera from,
                                   VirtualCamera to, TransitionOptions options,
                                   float smoothTime = 0.1f)
        {
            TransitionConfig transitionConfig =
                new TransitionConfig(options, smoothTime, from ? from.transform : cameraController.transform,
                                     from && from.IsOrthographic, 0, 0,
                                     0, to);

            // TODO: Add back support for transitioning fov and clipping planes
            // TransitionConfig transitionConfig = new TransitionConfig(
            // 	options,
            // 	smoothTime,
            // 	from.transform,
            // 	from.IsOrthographic,
            // 	from.FOV,
            // 	from.NearClipPlane,
            // 	from.FarClipPlane,
            // 	to
            // );

            toVirtualCamera = to;

            CreateTransition(cameraController, transitionConfig);
            StartTransition();
        }
    }
}
