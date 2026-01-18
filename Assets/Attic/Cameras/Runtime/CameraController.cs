using Attic.Cameras.VirtualCameras;
using UnityEngine;

namespace Attic.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private VirtualCameraManager virtualCameraManager;
        [SerializeField] private string initialCameraId;

        public Camera Camera { get; private set; }

        private void Awake()
        {
            Camera = GetComponent<Camera>();
        }

        private void Start()
        {
            if (virtualCameraManager == null)
            {
                virtualCameraManager = FindFirstObjectByType<VirtualCameraManager>();
            }

            virtualCameraManager.SetInitialCamera(this, initialCameraId);
        }

        /// <summary>
        /// Project a vector on the camera's plane. Handy to use for input.
        /// </summary>
        /// <param name="vector"></param>
        public void ProjectOnPlane(ref Vector3 vector)
        {
            Quaternion angleAlongWorldUp =
                Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);

            vector = angleAlongWorldUp * vector;
        }
    }
}
