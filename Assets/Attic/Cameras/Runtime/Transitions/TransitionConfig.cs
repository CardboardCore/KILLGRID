using Attic.Cameras.VirtualCameras;
using UnityEngine;

namespace Attic.Cameras.Transitions
{
    internal class TransitionConfig
    {
        internal readonly TransitionOptions TransitionOptions;
        internal readonly float TransitionTime;

        internal readonly bool IsInstant;

        internal readonly Transform From;

        internal readonly bool FromIsOrthographic;

        internal readonly float FromFOV;

        internal readonly float FromNearClipPlane;

        internal readonly float FromFarClipPlane;

        internal readonly VirtualCamera ToCamera;

        internal TransitionConfig(TransitionOptions options, float transitionTime, Transform from, bool fromIsOrthographic,
                                  float fromFOV, float fromNearClipPlane, float fromFarClipPlane, VirtualCamera toCamera)
        {
            TransitionOptions = options;
            TransitionTime = transitionTime;

            IsInstant = options.HasFlag(TransitionOptions.Instant);

            From = from;

            FromIsOrthographic = fromIsOrthographic;

            FromNearClipPlane = fromNearClipPlane;
            FromFarClipPlane = fromFarClipPlane;
            FromFOV = fromFOV;

            ToCamera = toCamera;
        }

        internal TransitionConfig CreateInterruptedConfig(Transform newStart, bool isOrthographic, float fieldOfView)
        {
            return new TransitionConfig(
                TransitionOptions,
                TransitionTime,
                newStart,
                isOrthographic,
                fieldOfView,
                FromNearClipPlane,
                FromFarClipPlane,
                ToCamera
            );
        }
    }
}
