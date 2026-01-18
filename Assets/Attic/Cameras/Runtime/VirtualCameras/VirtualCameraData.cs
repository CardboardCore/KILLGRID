using System;
using Attic.Cameras.Modules;
using UnityEngine;

namespace Attic.Cameras.VirtualCameras
{
    [Serializable]
    internal class VirtualCameraData
    {
        [SerializeField] private float x;
        [SerializeField] private float y;
        [SerializeField] private float z;

        [SerializeField] private float eulerX;
        [SerializeField] private float eulerY;
        [SerializeField] private float eulerZ;

        [SerializeField] private string id;

        [SerializeField] private bool isOrthographic;
        [SerializeField] private float fov = 60;

        [SerializeField] private float nearClipPlane = 0.3f;
        [SerializeField] private float farClipPlane = 1000;

        [SerializeField] private CameraModule[] modules = Array.Empty<CameraModule>();

        internal Vector3 Position
        {
            get => new Vector3(x, y, z);
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }

        internal Vector3 Euler
        {
            get => new Vector3(eulerX, eulerY, eulerZ);
            set
            {
                eulerX = value.x;
                eulerY = value.y;
                eulerZ = value.z;
            }
        }

        internal string ID => id;
        internal bool IsOrthographic => isOrthographic;
        internal float FOV => fov;
        internal float NearClipPlane => nearClipPlane;
        internal float FarClipPlane => farClipPlane;

        public VirtualCameraData()
        {
        }

        public VirtualCameraData(float x, float y, float z, float eulerX, float eulerY,
                                 float eulerZ, string id, bool isOrthographic, float fov,
                                 float nearClipPlane, float farClipPlane)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            this.eulerX = eulerX;
            this.eulerY = eulerY;
            this.eulerZ = eulerZ;

            this.id = id;
            this.isOrthographic = isOrthographic;
            this.fov = fov;
            this.nearClipPlane = nearClipPlane;
            this.farClipPlane = farClipPlane;
        }

        internal CameraModule[] Modules
        {
            get => modules;
            set => modules = value;
        }

        public void OverwriteId(string newId)
        {
            id = newId;
        }

        public void OverwriteFOV(float newFOV)
        {
            fov = newFOV;
        }
    }
}
