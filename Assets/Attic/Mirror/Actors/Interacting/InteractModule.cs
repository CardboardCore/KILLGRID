using System;
using Attic.Utilities;
using UnityEngine;

namespace Attic.Mirror.Actors.Interacting
{
    public abstract class InteractModule : AtticNetworkBehaviour
    {
        [SerializeField] private InteractRequirement interactRequirement;
        [SerializeField] private InteractInput interactInput;
        [SerializeField] private InteractionType interactionType;

        private bool isInteractingEnabled;

        protected InteractableActor Owner { get; private set; }
        protected Actor InteractingActor { get; private set; }

        public bool IsInteractedWith => InteractingActor;
        public InteractInput InteractInput => interactInput;
        public InteractionType InteractionType => interactionType;

        public event Action InteractBeginEvent;
        public event Action InteractEndEvent;

        protected override void OnInjected()
        {
            Owner = GetComponent<InteractableActor>();

            if (interactRequirement)
            {
                interactRequirement.Initialize(Owner);
            }
        }

        protected override void OnReleased()
        {

        }

        public bool CanInteract(Actor interactingActor, InteractInput interactInput)
        {
            if (!interactingActor || !interactingActor.isLocalPlayer || this.interactInput != interactInput || IsInteractedWith)
            {
                return false;
            }

            bool canInteract = !interactRequirement || interactRequirement.CanInteract(interactingActor);

            if (canInteract)
            {
                Owner.SetHighlightColor(interactInput);
            }

            return canInteract;
        }

        /// <summary>
        /// Begin interaction with the interactable actor. Only runs locally on interacting client.
        /// </summary>
        /// <param name="interactingActor"></param>
        public virtual void OnInteractBegin(Actor interactingActor)
        {
            if (interactingActor == null)
            {
                Log.Error($"Interactable {Owner.name} is being interacted with by a null actor");
                return;
            }

            if (interactingActor == InteractingActor)
            {
                Log.Error($"Interactable {Owner.name} is already being interacted with by {InteractingActor.name}");
                return;
            }

            if (IsInteractedWith)
            {
                Log.Error($"{interactingActor.name} is trying to interact with {Owner.name} but it is already being interacted with by {InteractingActor.name}");
                return;
            }

            InteractingActor = interactingActor;

            InteractBeginEvent?.Invoke();
        }

        /// <summary>
        /// End interaction with the interactable actor. Only runs locally on interacting client.
        /// </summary>
        public virtual void OnInteractEnd()
        {
            if (!InteractingActor)
            {
                Log.Warn($"{Owner.name} is not being interacted with while trying to end interaction");
            }

            InteractingActor = null;

            InteractEndEvent?.Invoke();
        }

        public virtual void OnResetInteractable()
        {

        }
    }
}
