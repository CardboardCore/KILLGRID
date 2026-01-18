using System;
using Attic.Cameras.VirtualCameras;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Attic.Cameras.Modules
{
    [Serializable]
    public abstract class CameraModule : ScriptableObject
    {
#if UNITY_EDITOR
        // Used for editor tools
        // ReSharper disable once NotAccessedField.Local
        [SerializeField]
        private bool queuedForRemoval;
#endif

        [SerializeField]
        private VirtualCamera myVirtualCamera;

        protected VirtualCamera MyVirtualCamera => myVirtualCamera;
        protected Transform Transform => myVirtualCamera.transform;

        public Transform Parent => Transform.parent;

        /// <summary>
        /// Stores data used for saving/loading this module
        /// </summary>
        internal void Setup(VirtualCamera virtualCamera)
        {
            myVirtualCamera = virtualCamera;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        internal void DrawGizmos()
        {
            OnDrawGizmos();
        }

        internal void Start()
        {
            OnStart();
        }

        internal void Stop()
        {
            OnStop();
        }

        internal void Activate()
        {
            OnActivate();
        }

        internal void Deactivate()
        {
            OnDeactivate();
        }

        internal void Tick(float deltaTime)
        {
            OnTick(deltaTime);
        }

        internal void FixedTick(float fixedDeltaTime)
        {
            OnFixedTick(fixedDeltaTime);
        }

        protected virtual void OnDrawGizmos() { }
        /// <summary>
        /// Called once the owning virtual camera of this module is enabled
        /// </summary>
        protected abstract void OnStart();
        /// <summary>
        /// Called once the owning virtual camera of this module is disabled
        /// </summary>
        protected abstract void OnStop();
        /// <summary>
        /// Called once the transition into the owning virtual camera of this module is finished
        /// </summary>
        protected virtual void OnActivate() { }
        /// <summary>
        /// Called once the transition out of the owning virtual camera of this module is started
        /// </summary>
        protected virtual void OnDeactivate() { }
        /// <summary>
        /// Called every frame while the owning virtual camera of this module is enabled
        /// </summary>
        /// <param name="deltaTime"></param>
        protected virtual void OnTick(float deltaTime) { }
        protected virtual void OnFixedTick(float fixedDeltaTime) { }
    }
}
