using Attic.Cameras.Modules;
using UnityEngine;

namespace Run.Cameras
{
    public class DeltaBasedRotationModule : CameraModule
    {
        [SerializeField] private bool smoothing;
        [SerializeField] private float smoothTime = 0.01f;
        [SerializeField] private float maxSpeed = 1000f;
        [SerializeField] private float minRotationX = -80f;
        [SerializeField] private float maxRotationX = 80f;

        private Transform referenceTransform;

        private Vector2 targetRotation;
        private Vector2 currentRotation;
        private Vector3 footstepOffsetRotation;
        private Vector3 strafeOffsetRotation;

        private float upsideDownRotation;

        private float xVel;
        private float yVel;

        private bool isEnabled;

        protected override void OnStart()
        {
            Enable();
        }

        protected override void OnStop()
        {

        }

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);

            if (!referenceTransform)
            {
                return;
            }

            if (isEnabled)
            {
                // Rotate the camera based on rotation value
                targetRotation.x = Mathf.Clamp(targetRotation.x, -maxRotationX, -minRotationX);
            }

            if (smoothing)
            {
                // Smoothly interpolate the current rotation towards the target rotation
                currentRotation.x = Mathf.SmoothDampAngle(currentRotation.x, targetRotation.x, ref xVel, smoothTime, maxSpeed);
                currentRotation.y = Mathf.SmoothDampAngle(currentRotation.y, targetRotation.y, ref yVel, smoothTime, maxSpeed);
            }
            else
            {
                currentRotation = targetRotation;
            }

            // Apply the footstep offset rotation
            currentRotation.x += footstepOffsetRotation.x;
            currentRotation.y += footstepOffsetRotation.y;

            float z = footstepOffsetRotation.z;

            currentRotation.x += strafeOffsetRotation.x;
            currentRotation.y += strafeOffsetRotation.y;
            z += strafeOffsetRotation.z;

            Quaternion currentRotationQuat = Quaternion.Euler(currentRotation.x, currentRotation.y, z);

            Quaternion worldRotation = referenceTransform.rotation * currentRotationQuat;

            Transform.rotation = worldRotation;
        }

        public void AddDelta(Vector2 deltaInput)
        {
            targetRotation.x -= deltaInput.y;
            targetRotation.y += deltaInput.x;
        }

        public void SetRotation(Quaternion rotation, bool instant = false)
        {
            targetRotation = new Vector2(rotation.eulerAngles.x, rotation.eulerAngles.y);

            if (instant)
            {
                currentRotation = targetRotation;
            }
        }

        public void SetRefernceTransform(Transform referenceTransform)
        {
            this.referenceTransform = referenceTransform;
        }

        public void SetFootstepOffset(Vector3 offset)
        {
            footstepOffsetRotation = offset;
        }

        public void SetStrafeOffset(Vector3 offset)
        {
            strafeOffsetRotation = offset;
        }

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }
    }
}
