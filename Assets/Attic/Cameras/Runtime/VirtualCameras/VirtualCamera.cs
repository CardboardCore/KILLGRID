using System;
using System.Collections.Generic;
using System.Linq;
using Attic.Cameras.Modules;
using UnityEditor;
using UnityEngine;

namespace Attic.Cameras.VirtualCameras
{
    public class VirtualCamera : MonoBehaviour
    {
        private enum VirtualCameraState
        {
            Idle,
            Active
        }

        // Used in editor only
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private bool isNameLocked;

        [SerializeField] private VirtualCameraData data = new VirtualCameraData();
        [SerializeField] private VirtualCameraManager myVirtualCameraManager;

        private VirtualCameraState virtualCameraState;
        private CameraController cameraController;

        public float FOV => data.FOV;
        public string Id => data.ID;
        public bool IsOrthographic => data.IsOrthographic;

#region Lifecycle

        internal void Enable()
        {
            if (data == null || data.Modules == null || data.Modules.Length == 0)
            {
                data = new VirtualCameraData();
                data.Modules = new CameraModule[0];
                return;
            }

            for (int i = 0; i < data.Modules.Length; i++)
            {
                data.Modules[i].Start();
            }
        }

        internal void Disable()
        {
            for (int i = 0; i < data.Modules.Length; i++)
            {
                data.Modules[i].Stop();
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < data.Modules.Length; i++)
            {
                data.Modules[i].Tick(Time.deltaTime);
            }

            if (virtualCameraState != VirtualCameraState.Active)
            {
                return;
            }

            cameraController.transform.position = transform.position;
            cameraController.transform.rotation = transform.rotation;

            // TODO: Add back support for transitioning fov and clipping planes
            // TODO: Find a way to access data from modules
            cameraController.Camera.fieldOfView = data.FOV;
            cameraController.Camera.nearClipPlane = data.NearClipPlane;
            cameraController.Camera.farClipPlane = data.FarClipPlane;
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < data.Modules.Length; i++)
            {
                data.Modules[i].FixedTick(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Activate this VirtualCamera. This means it will start applying transform values to the given CameraController.
        /// </summary>
        /// <param name="cameraController"></param>
        internal void Activate(CameraController cameraController)
        {
            this.cameraController = cameraController;
            virtualCameraState = VirtualCameraState.Active;

            for (int i = 0; i < data.Modules.Length; i++)
            {
                data.Modules[i].Activate();
            }
        }

        /// <summary>
        /// Deactivate this VirtualCamera. It'll release the CameraController.
        /// </summary>
        internal void Deactivate()
        {
            for (int i = 0; i < data.Modules.Length; i++)
            {
                data.Modules[i].Deactivate();
            }

            virtualCameraState = VirtualCameraState.Idle;
            cameraController = null;
        }

#endregion

#region Saving

        public void Save()
        {
            myVirtualCameraManager ??= FindObjectOfType<VirtualCameraManager>();

            if (myVirtualCameraManager == null)
            {
                throw new Exception(
                    "No Virtual Camera Manager was found in the scene! Please add it.");
            }

            // This method will call AssetDatabase.Refresh when in editor, so no need to do it here
            // Will trigger a compile and reload of assets
            myVirtualCameraManager.OnVirtualCameraSaved();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Adds a new module to this v-cam. Requires <see cref="data"/> to be set prior to adding modules.
        /// </summary>
        /// <param name="module">Instance of a <see cref="CameraModule"/></param>
        public void AddModule<T>(T module) where T : CameraModule
        {
            module.Setup(this);

            List<CameraModule> modulesList = data.Modules.ToList();
            modulesList.Add(module);
            data.Modules = modulesList.ToArray();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveModuleAt(int index)
        {
            List<CameraModule> modulesList = data.Modules.ToList();

            if (modulesList[index] == null)
            {
                return;
            }

            modulesList.RemoveAt(index);

            data.Modules = modulesList.ToArray();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

#endregion

#region Editor Only

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameObject.scene.name == null)
            {
                return;
            }

            // OnValidate is called before loading of scene is finished, so we find objects after the scene is declared loaded
            myVirtualCameraManager = FindObjectOfType<VirtualCameraManager>();

            if (myVirtualCameraManager == null)
            {
                throw new Exception(
                    "No Virtual Camera Manager was found in the scene! Please add it.");
            }

            myVirtualCameraManager.RegisterVirtualCamera(this);

            DrawIcon();
        }

        private void OnDrawGizmos()
        {
            DrawUnselectedGizmos();

            if (IsSelected() && data != null && data.Modules != null)
            {
                for (int i = 0; i < data.Modules.Length; i++)
                {
                    if (data.Modules[i] == null)
                    {
                        continue;
                    }

                    data.Modules[i].DrawGizmos();
                }
            }
        }

        private void DrawUnselectedGizmos()
        {
            Color color = Color.cyan;
            color.a = IsSelected() ? 1f : 0.1f;
            Gizmos.color = color;

            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, 60f, 5f, 0f, 1f);
            Gizmos.matrix = originalMatrix;
        }

        private void DrawIcon()
        {
#if UNITY_2021_2_OR_NEWER
            Texture2D icon = (Texture2D)Resources.Load("virtual-camera");
            EditorGUIUtility.SetIconForObject(gameObject, icon);
#else
			Texture2D icon = (Texture2D) Resources.Load("virtual-camera");

			Type editorGUIUtilityType = typeof(UnityEditor.EditorGUIUtility);
			const BindingFlags bindingFlags =
 BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
			object[] args = { gameObject, icon };
			editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
#endif
        }

        private bool IsSelected()
        {
            return Selection.activeGameObject == gameObject;
        }

        /// <summary>
        /// Editor-Time helper method to set references if they get lost, like when doing a refactor on serialized fields.
        /// </summary>
        public void RefreshModules()
        {
            for (int i = 0; i < data.Modules.Length; i++)
            {
                if (data.Modules[i] == null)
                {
                    continue;
                }

                data.Modules[i].Setup(this);
            }

            EditorUtility.SetDirty(this);
        }
#endif

#endregion

        public bool TryGetCameraModule<T>(out T cameraModule) where T : CameraModule
        {
            foreach (CameraModule module in data.Modules)
            {
                if (module.GetType() == typeof(T))
                {
                    cameraModule = (T)module;

                    return true;
                }
            }

            cameraModule = null;

            return false;
        }

        public void OverwriteId(string newId)
        {
            data.OverwriteId(newId);
        }

        public void OverwriteFOV(float newFov)
        {
            data.OverwriteFOV(newFov);
        }
    }
}
