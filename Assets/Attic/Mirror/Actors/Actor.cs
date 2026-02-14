using System.Collections;
using System.Collections.Generic;
using Attic.Mirror.Actors.Components;
using Attic.DI;
using Attic.Utilities;
using Attic.Utils.Invoking;
using Mirror;
using UnityEngine;

namespace Attic.Mirror.Actors
{
    public class Actor : AtticNetworkBehaviour
    {
        // ReSharper disable once UnassignedField.Global
        // ReSharper disable once InconsistentNaming
        [Inject] private ActorRegistry actorRegistry;
        [Inject] private InvokeWrapper invokeWrapper;

        private ActorComponent[] actorComponents;
        private Coroutine removeOwnershipCoroutine;
        private bool isMarkedForDestruction;

        protected override InjectTiming MyInjectTiming => InjectTiming.Client;

        public ActorRegistry ActorRegistry => actorRegistry;

        protected override void OnInjected()
        {
            actorRegistry.RegisterActor(this);
            actorComponents = GetComponents<ActorComponent>();
        }

        protected override void OnReleased()
        {
            actorRegistry.UnregisterActor(this);
        }

        [Command(requiresAuthority = false)]
        private void Cmd_SetOwnership(uint owningActorNetId)
        {
            Actor owningActor = ActorRegistry.GetActor(owningActorNetId);

            if (removeOwnershipCoroutine != null)
            {
                StopCoroutine(removeOwnershipCoroutine);
                removeOwnershipCoroutine = null;
            }

            netIdentity.RemoveClientAuthority();
            netIdentity.AssignClientAuthority(owningActor.netIdentity.connectionToClient);

            Log.Write($"Set ownership to {owningActor.name}");
        }

        [Command]
        private void Cmd_ClearOwnership()
        {
            if (removeOwnershipCoroutine != null)
            {
                StopCoroutine(removeOwnershipCoroutine);
                removeOwnershipCoroutine = null;
            }

            removeOwnershipCoroutine = StartCoroutine(DelayedRemoveClientAuthority());
        }

        private IEnumerator DelayedRemoveClientAuthority()
        {
            yield return new WaitForSeconds(5f);

            netIdentity.RemoveClientAuthority();

            Log.Write($"Removed ownership from {name}");
        }

        public T GetActorComponent<T>() where T : ActorComponent
        {
            foreach (ActorComponent actorComponent in actorComponents)
            {
                if (actorComponent is T component)
                {
                    return component;
                }
            }

            return null;
        }

        public T[] GetActorComponents<T>() where T : ActorComponent
        {
            List<T> components = new List<T>();

            foreach (ActorComponent actorComponent in actorComponents)
            {
                if (actorComponent is T component)
                {
                    components.Add(component);
                }
            }

            return components.ToArray();
        }

        public void DisableView()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }

        [Client]
        public void RequestTakeOwnership(Actor actor)
        {
            Cmd_SetOwnership(actor.netId);
        }

        [Client]
        public void RequestRemoveOwnership()
        {
            Cmd_ClearOwnership();
        }

        public void Cleanup()
        {
            foreach (ActorComponent actorComponent in actorComponents)
            {
                actorComponent.Cleanup();
            }
        }

        [Server]
        public void MarkForDestruction()
        {
            if (isMarkedForDestruction)
            {
                return;
            }

            isMarkedForDestruction = true;

            invokeWrapper.Invoke(() => {
                NetworkServer.Destroy(gameObject);
            }, 5f);
        }
    }
}
