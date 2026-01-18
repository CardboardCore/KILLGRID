using Attic.Audio;
using Attic.Mirror.Actors.Interacting;
using Attic.DI;
using Attic.Utilities;
using UnityEngine;

namespace Attic.Mirror.Actors.Components
{
    public class ActorCarry : ActorComponent
    {
        [Inject] private ActorRegistry actorRegistry;
        [Inject] private AudioManager audioManager;

        [SerializeField] private Transform carryPoint;

        private InteractableActor carriedActor;

        public bool IsCarrying => carriedActor;
        public InteractableActor CarriedActor => carriedActor;
        public Transform CarryPoint => carryPoint;

        private void OnDrawGizmos()
        {
            if (!carryPoint)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(carryPoint.position, 0.1f);
        }

        private void LateUpdate()
        {
            if (!carriedActor)
            {
                return;
            }

            carriedActor.SetPosition(carryPoint.position);
            carriedActor.SetRotation(carryPoint.rotation);
        }

        public void Carry(InteractableActor interactableActor)
        {
            if (carriedActor)
            {
                return;
            }

            carriedActor = interactableActor;
            carriedActor.DisableColliders();

            audioManager.PlaySFX("grab", new PlaySFXOptions(transform, 1f, 1f));
        }

        public void Drop()
        {
            if (!carriedActor)
            {
                Log.Warn("No actor to drop");
                return;
            }

            Log.Write($"Dropping actor {carriedActor.gameObject.name}");
            carriedActor.EnableColliders();
            carriedActor.ResetInteractable();

            carriedActor = null;

            audioManager.PlaySFX("grab", new PlaySFXOptions(transform, 1f, 0.5f));
        }
    }
}
