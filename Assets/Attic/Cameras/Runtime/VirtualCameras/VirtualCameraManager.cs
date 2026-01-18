using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Attic.Cameras.Transitions;
using Attic.DI;
using UnityEditor;
using UnityEngine;

#if CC_DI
#endif

namespace Attic.Cameras.VirtualCameras
{
#if CC_DI
    [Injectable, RequireComponent(typeof(TransitionManager))]
#endif
    public class VirtualCameraManager : MonoBehaviour
    {
        [SerializeField] private List<VirtualCamera> virtualCameras = new List<VirtualCamera>();

        private TransitionManager transitionManager;
        private VirtualCamera currentVirtualCamera;

        public CameraController CameraController { get; private set; }
        public ReadOnlyCollection<VirtualCamera> VirtualCameras => virtualCameras.AsReadOnly();

        private void OnDrawGizmos()
        {
            for (int i = virtualCameras.Count - 1; i >= 0; i--)
            {
                if (virtualCameras[i] != null)
                {
                    continue;
                }

                virtualCameras.RemoveAt(i);
            }
        }

        private void Awake()
        {
            transitionManager = GetComponent<TransitionManager>();
        }

        private void OnEnable()
        {
            for (int i = 0; i < virtualCameras.Count; i++)
            {
                // TODO: This happens when cameras are scattered over different scenes, fix!
                if (virtualCameras[i] == null)
                {
                    continue;
                }

                virtualCameras[i].Enable();
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < virtualCameras.Count; i++)
            {
                // TODO: This happens when cameras are scattered over different scenes, fix!
                if (virtualCameras[i] == null)
                {
                    continue;
                }

                virtualCameras[i].Disable();
            }
        }

        internal void SetInitialCamera(CameraController cameraController, string id)
        {
            if (!TryGetVirtualCamera(id, out VirtualCamera virtualCamera))
            {
                throw new Exception($"Unable to find Virtual Camera with Id {id}");
            }

            virtualCamera.Activate(cameraController);

            CameraController = cameraController;
            currentVirtualCamera = virtualCamera;
        }

        private void OnTransitionFinishedEvent(VirtualCamera virtualCamera)
        {
            if (virtualCamera != currentVirtualCamera)
            {
                throw new Exception("A transition was finished into an incorrect V-Cam!");
            }

            transitionManager.OnTransitionFinishedEvent -= OnTransitionFinishedEvent;
            virtualCamera.Activate(CameraController);
        }

        public void DoTransition(string id, TransitionOptions options, float duration = 0.2f)
        {
            if (!TryGetVirtualCamera(id, out VirtualCamera newVirtualCamera))
            {
                throw new Exception($"Unable to find Virtual Camera with Id {id}");
            }

            DoTransition(newVirtualCamera, options, duration);
        }

        public void DoTransition(VirtualCamera newVirtualCamera, TransitionOptions options, float duration = 0.2f)
        {
            if (newVirtualCamera == currentVirtualCamera)
            {
                // No need to do transition when trying to activate already active v-cam
                Debug.LogWarning("Trying to transition to already active V-Cam");

                return;
            }

            currentVirtualCamera.Deactivate();

            VirtualCamera previousCamera = currentVirtualCamera;

            // Set the new virtual camera at the start of the transition instead of when transition is finished, as this will
            // help interrupting this transition, so we know where we're supposed to be headed
            currentVirtualCamera = newVirtualCamera;

            transitionManager.OnTransitionFinishedEvent += OnTransitionFinishedEvent;
            transitionManager.DoTransition(CameraController, previousCamera, newVirtualCamera,
                                           options, duration);
        }

        public void SaveAllCameras()
        {
            for (int i = 0; i < virtualCameras.Count; i++)
            {
                virtualCameras[i].Save();
            }

            OnVirtualCameraSaved();
        }

        internal void OnVirtualCameraSaved()
        {
            VirtualCameraIdsGenerator.Write(gameObject.scene.name, this);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

#if UNITY_EDITOR
        public void Refresh()
        {
            for (int i = this.virtualCameras.Count - 1; i >= 0; i--)
            {
                this.virtualCameras.RemoveAt(i);
            }

            VirtualCamera[] cameras = FindObjectsByType<VirtualCamera>(FindObjectsSortMode.None);

            foreach (VirtualCamera virtualCamera in cameras)
            {
                RegisterVirtualCamera(virtualCamera);
            }

            EditorUtility.SetDirty(this);
        }
#endif

        public void RegisterVirtualCamera(VirtualCamera virtualCamera)
        {
            if (virtualCameras.Contains(virtualCamera))
            {
                return;
            }

#if UNITY_EDITOR
            virtualCamera.RefreshModules();
#endif

            virtualCameras.Add(virtualCamera);
        }

        public bool TryGetVirtualCamera(string id, out VirtualCamera virtualCamera)
        {
            virtualCamera = virtualCameras.FirstOrDefault(t => t.Id.Equals(id));

            if (virtualCamera == null)
            {
                // TODO: This is here because of cross-scene v-cams... fix!
                VirtualCamera[] cameras = FindObjectsByType<VirtualCamera>(FindObjectsSortMode.None);

                foreach (VirtualCamera vCam in cameras)
                {
                    RegisterVirtualCamera(vCam);
                }

                virtualCamera = cameras.FirstOrDefault(t => t.Id.Equals(id));
            }

            return virtualCamera != null;
        }

        public void ClearAllCamerasAndFindThemAgain()
        {
            virtualCameras.Clear();
            virtualCameras = FindObjectsByType<VirtualCamera>(FindObjectsSortMode.None).ToList();
        }
    }
}
