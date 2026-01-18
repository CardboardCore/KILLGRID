using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attic.Utilities;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Attic.Mirror.Actors.Interacting
{

    public class InteractableActor : Actor
    {
        [Header("Project References")]
        [SerializeField] private InteractColorConfig interactColorConfig;

        [Header("References")]
        [SerializeField] private GameObject interactableCollidersContainer;

        [Header("Optional References")]
        [SerializeField] private new Rigidbody rigidbody;

        [Header("Settings")]
        [SerializeField] private bool dependsOnParentInteractable;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.1f;

        private InteractModule[] interactModules;
        private InteractableActor parentInteractableActor;

        [SyncVar] private bool isInteractingEnabled = true;

        private Collider[] colliders;
        private Outline[] outlines;

        private Sequence placedActorSequence;

        public bool IsShowingHighlight { get; private set; }
        /// <summary>
        /// The actor currently interacting with this interactable
        /// </summary>
        public Actor ContinuousInteractingActor { get; private set; }

        public event Action EnableInteractionEvent;
        public event Action DisableInteractionEvent;

        public event Action<InteractableActor, InteractInput, Actor> InteractEvent;
        public event Action<InteractableActor> ForceStopInteractEvent;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
        }

        protected override void OnInjected()
        {
            base.OnInjected();

            // Setup interact modules and map possible interact modes
            interactModules = GetComponents<InteractModule>();

            // Setup colliders
            colliders = GetComponentsInChildren<Collider>();

            // Setup parent interactable dependency
            if (dependsOnParentInteractable && transform.parent != null)
            {
                parentInteractableActor = transform.parent.GetComponentInParent<InteractableActor>();

                if (!parentInteractableActor)
                {
                    Log.Warn($"Interactable {name} depends on parent interactable, but no parent interactable found.");
                }
                else
                {
                    parentInteractableActor.InteractEvent += OnParentInteract;
                    DisableInteraction();
                }
            }

            // Setup outlines
            outlines = GetComponentsInChildren<Outline>();
            foreach (Outline outline in outlines)
            {
                outline.enabled = false;
                // TODO: Check for prewarming?
            }
        }

        private void OnParentInteract(InteractableActor interactableActor, InteractInput interactInput, Actor arg3)
        {
            parentInteractableActor.InteractEvent -= OnParentInteract;
            EnableInteraction();
        }

        [Command(requiresAuthority = false)]
        private void Cmd_Interact(uint actorNetId, InteractInput interactInput)
        {
            Log.Write($"Relay: Actor with NetId \"{actorNetId}\" is interacting with {name}");
            Rpc_Interact(actorNetId, interactInput);
        }

        [ClientRpc]
        private void Rpc_Interact(uint actorNetId, InteractInput interactInput)
        {
            Actor interactingActor = ActorRegistry.GetActor(actorNetId);

            if (!interactingActor)
            {
                Log.Error($"Actor with NetId \"{actorNetId}\" is interacting with {name}, but actor not found in registry.");
                return;
            }

            // Because we already started interacting locally, we don't want to start interacting again
            if (interactingActor.isLocalPlayer)
            {
                return;
            }

            InteractInternal(interactingActor, interactInput);
        }

        [Command(requiresAuthority = false)]
        private void Cmd_ForceStopInteract(uint actorNetId, InteractInput interactInput)
        {
            Rpc_ForceStopInteract(actorNetId, interactInput);
        }

        [ClientRpc]
        private void Rpc_ForceStopInteract(uint actorNetId, InteractInput interactInput)
        {
            Actor interactingActor = ActorRegistry.GetActor(actorNetId);

            if (interactingActor != ContinuousInteractingActor)
            {
                Log.Warn($"Trying to force stop interaction with actor {actorNetId}, but actor is not interacting with this interactable.");
                return;
            }

            // Because we already stopped interacting locally, we don't want to start interacting again
            if (interactingActor.isLocalPlayer)
            {
                return;
            }

            ForceStopInteractInternal(interactInput);
        }

        private void InteractInternal(Actor interactingActor, InteractInput interactInput)
        {
            List<InteractModule> eligibleInteractModules = new List<InteractModule>();

            foreach (InteractModule interactModule in interactModules)
            {
                if (!interactModule.CanInteract(interactingActor, interactInput))
                {
                    continue;
                }

                eligibleInteractModules.Add(interactModule);
            }

            foreach (InteractModule eligibleInteractModule in eligibleInteractModules)
            {
                switch (eligibleInteractModule.InteractionType)
                {
                    case InteractionType.Single:

                        eligibleInteractModule.OnInteractBegin(interactingActor);
                        eligibleInteractModule.OnInteractEnd();

                        HideHighlight();

                        break;

                    case InteractionType.OneOff:

                        eligibleInteractModule.OnInteractBegin(interactingActor);

                        DisableInteraction();

                        eligibleInteractModule.OnInteractEnd();

                        break;

                    case InteractionType.Toggle:

                        if (eligibleInteractModule.IsInteractedWith)
                        {
                            eligibleInteractModule.OnInteractEnd();
                        }
                        else
                        {
                            eligibleInteractModule.OnInteractBegin(interactingActor);
                        }

                        break;

                    case InteractionType.Continuous:

                        if (eligibleInteractModule.IsInteractedWith && interactingActor != ContinuousInteractingActor)
                        {
                            break;
                        }

                        ContinuousInteractingActor = interactingActor;

                        eligibleInteractModule.OnInteractBegin(interactingActor);

                        break;
                }
            }

            InteractEvent?.Invoke(this, interactInput, interactingActor);
        }

        private void ForceStopInteractInternal(InteractInput interactInput)
        {
            List<InteractModule> eligibleInteractModules = new List<InteractModule>();

            foreach (InteractModule interactModule in interactModules)
            {
                if (interactModule.InteractInput != interactInput)
                {
                    continue;
                }

                eligibleInteractModules.Add(interactModule);
            }

            foreach (InteractModule eligibleInteractModule in eligibleInteractModules)
            {
                switch (eligibleInteractModule.InteractionType)
                {
                    case InteractionType.OneOff:

                        eligibleInteractModule.OnResetInteractable();

                        break;

                    case InteractionType.Continuous:

                        if (!eligibleInteractModule.IsInteractedWith)
                        {
                            return;
                        }

                        eligibleInteractModule.OnInteractEnd();

                        ContinuousInteractingActor = null;

                        break;
                }
            }

            ForceStopInteractEvent?.Invoke(this);
        }

        public void RequestInteract(Actor interactingActor, InteractInput interactInput)
        {
            if (interactingActor.isLocalPlayer)
            {
                InteractInternal(interactingActor, interactInput);
                Cmd_Interact(interactingActor.netId, interactInput);
            }
        }

        /// <summary>
        /// Will force stop interaction after 1 frame delay, to keep RPCs in the correct order.
        /// Can be called immediately as well, but should only be done on an object which is not being interacted with
        /// (e.g. when dropping 1 object and grabbing another, the dropped object should force stop interaction immediately instead).
        /// </summary>
        /// <param name="interactInput"></param>
        /// <param name="immediate"></param>
        public void RequestForceStopInteract(InteractInput interactInput, bool immediate = false)
        {
            if (!ContinuousInteractingActor)
            {
                Log.Warn("Trying to force stop interact, but no actor is interacting with this interactable.");
                return;
            }

            if (ContinuousInteractingActor.isLocalPlayer)
            {
                if (immediate)
                {
                    Cmd_ForceStopInteract(ContinuousInteractingActor.netId, interactInput);
                    ForceStopInteractInternal(interactInput);
                }
                else
                {
                    StartCoroutine(WaitForForceStopInteract(ContinuousInteractingActor, interactInput));
                }
            }

            return;

            IEnumerator WaitForForceStopInteract(Actor continuousInteractingActor, InteractInput interactInput)
            {
                yield return null;

                Cmd_ForceStopInteract(continuousInteractingActor.netId, interactInput);
                ForceStopInteractInternal(interactInput);
            }
        }

        public bool CanInteract(Actor interactingActor)
        {
            if (interactModules == null || !isInteractingEnabled)
            {
                return false;
            }

            foreach (InteractModule interactModule in interactModules)
            {
                if (interactModule.CanInteract(interactingActor, InteractInput.Default)
                    || interactModule.CanInteract(interactingActor, InteractInput.Alternative))
                {
                    return true;
                }
            }

            return false;
        }

        public void ShowHighlight()
        {
            if (IsShowingHighlight)
            {
                return;
            }

            foreach (Outline outline in outlines)
            {
                outline.enabled = true;
            }

            IsShowingHighlight = true;
        }

        public void HideHighlight()
        {
            if (!IsShowingHighlight)
            {
                return;
            }

            foreach (Outline outline in outlines)
            {
                outline.enabled = false;
            }

            IsShowingHighlight = false;
        }

        public void SetHighlightColor(InteractInput interactInput)
        {
            Color color = interactColorConfig.GetColor(interactInput);

            foreach (Outline outline in outlines)
            {
                // outline.OutlineColor = color;
            }
        }

        public void EnableInteraction()
        {
            if (isInteractingEnabled)
            {
                return;
            }

            if (interactableCollidersContainer)
            {
                interactableCollidersContainer.SetActive(true);
            }

            isInteractingEnabled = true;

            EnableInteractionEvent?.Invoke();
        }

        public void DisableInteraction()
        {
            if (!isInteractingEnabled)
            {
                return;
            }

            if (interactableCollidersContainer)
            {
                interactableCollidersContainer.SetActive(false);
            }

            isInteractingEnabled = false;

            DisableInteractionEvent?.Invoke();
        }

        public void ResetInteractable()
        {
            foreach (InteractModule interactComponent in interactModules)
            {
                interactComponent.OnResetInteractable();
            }

            EnableInteraction();
        }

        public void EnableColliders()
        {
            foreach (Collider c in colliders)
            {
                c.enabled = true;
            }
        }

        public void DisableColliders()
        {
            foreach (Collider c in colliders)
            {
                c.enabled = false;
            }
        }

        public void SetKinematic(bool isKinematic)
        {
            if (!rigidbody)
            {
                return;
            }

            rigidbody.isKinematic = isKinematic;
        }

        public void SetPosition(Vector3 position)
        {
            if (!rigidbody)
            {
                transform.position = position;
                return;
            }

            if (rigidbody.isKinematic)
            {
                transform.position = position;
            }
            else
            {
                rigidbody.MovePosition(position);
            }
        }

        public void SetLocalPosition(Vector3 localPosition)
        {
            if (!rigidbody)
            {
                transform.localPosition = localPosition;
                return;
            }

            if (rigidbody.isKinematic)
            {
                transform.localPosition = localPosition;
            }
            else
            {
                rigidbody.MovePosition(transform.TransformPoint(localPosition));
            }
        }

        public void SetRotation(Quaternion rotation)
        {
            if (!rigidbody)
            {
                rigidbody.rotation = rotation;
                return;
            }

            if (rigidbody.isKinematic)
            {
                transform.rotation = rotation;
            }
            else
            {
                rigidbody.MoveRotation(rotation);
            }
        }

        public void SetPositionAndRotationAnimated(Vector3 targetPosition, Quaternion targetRotation)
        {
            bool wasInteractingEnabled = isInteractingEnabled;
            bool wasKinematic = rigidbody && rigidbody.isKinematic;

            DisableInteraction();

            placedActorSequence?.Kill();
            placedActorSequence = DOTween.Sequence();

            if (rigidbody)
            {
                rigidbody.isKinematic = true;
                placedActorSequence.Insert(0, rigidbody.DOMove(targetPosition, animationDuration).SetEase(Ease.Linear));
                placedActorSequence.Insert(0, rigidbody.DORotate(targetRotation.eulerAngles, animationDuration).SetEase(Ease.Linear));
            }
            else
            {
                placedActorSequence.Insert(0, transform.DOMove(targetPosition, animationDuration).SetEase(Ease.Linear));
                placedActorSequence.Insert(0, transform.DORotate(targetRotation.eulerAngles, animationDuration).SetEase(Ease.Linear));
            }

            placedActorSequence.OnComplete(() => {

                if (wasInteractingEnabled)
                {
                    EnableInteraction();
                }

                if (rigidbody)
                {
                    rigidbody.isKinematic = wasKinematic;
                }
            });
        }

        public T GetInteractModule<T>() where T : InteractModule
        {
            return interactModules.FirstOrDefault(x => x is T) as T;
        }
    }
}
