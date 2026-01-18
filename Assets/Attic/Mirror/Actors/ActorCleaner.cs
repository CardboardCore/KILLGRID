using System.Collections.Generic;
using Attic.Mirror.Actors.Interacting;
using Attic.DI;
using Attic.Utilities;
using Mirror;
using UnityEngine;

namespace Attic.Mirror.Actors
{
    public class ActorCleaner : AtticNetworkBehaviour
    {
        private class RemovalInfo
        {
            public Actor Actor { get; }
            public float Time { get; set; }

            public RemovalInfo(Actor actor, float time)
            {
                Actor = actor;
                Time = time;
            }
        }

        [Inject] private ActorRegistry actorRegistry;

        private readonly List<RemovalInfo> removalQueue = new List<RemovalInfo>();

        protected override void OnInjected()
        {
        }

        protected override void OnReleased()
        {
        }

        private void Update()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!isServer)
            {
                return;
            }

            for (int i = removalQueue.Count - 1; i >= 0; i--)
            {
                RemovalInfo removalInfo = removalQueue[i];
                removalInfo.Time -= Time.deltaTime;

                if (removalInfo.Time > 0f)
                {
                    continue;
                }

                if (!removalInfo.Actor)
                {
                    removalQueue.RemoveAt(i);
                    continue;
                }

                Log.Write($"Removing actor {removalInfo.Actor.name} from scene");

                NetworkServer.Destroy(removalInfo.Actor.gameObject);
                removalQueue.RemoveAt(i);
            }
        }

        [Command(requiresAuthority = false)]
        private void Cmd_QueueForRemoval(uint actorNetId)
        {
            Actor actor = actorRegistry.GetActor(actorNetId);

            if (!actor)
            {
                return;
            }

            Log.Write($"Queuing actor {actor.name} for removal");

            removalQueue.Add(new RemovalInfo(actor, 1.5f));

            Rpc_QueueForRemoval(actorNetId);
        }

        [ClientRpc]
        private void Rpc_QueueForRemoval(uint actorNetId)
        {
            Actor actor = actorRegistry.GetActor(actorNetId);

            if (!actor)
            {
                return;
            }

            Log.Write($"Hiding actor {actor.name} as it'll be removed soon");

            if (actor is InteractableActor interactableActor)
            {
                interactableActor.DisableInteraction();
                interactableActor.DisableColliders();
                interactableActor.DisableView();
                interactableActor.SetKinematic(true);
                interactableActor.SetPosition(new Vector3(-100, 0, 0));
            }
            else
            {
                actor.DisableView();
                actor.transform.position = new Vector3(-100, 0, 0);
            }
        }

        /// <summary>
        /// Request for an actor to be queued for removal.
        /// Will disable the actor's view and colliders, and set its position to a far away location.
        /// Will also call RequestForceStopInteract if the actor is an InteractableActor.
        /// </summary>
        /// <param name="actor"></param>
        public void RequestQueueForRemoval(Actor actor)
        {
            if (!actor)
            {
                return;
            }

            Log.Write($"Requesting queuing actor {actor.name} for removal");

            if (actor is InteractableActor interactableActor)
            {
                interactableActor.RequestForceStopInteract(InteractInput.Default, true);
                interactableActor.RequestForceStopInteract(InteractInput.Alternative, true);
                interactableActor.DisableInteraction();
                interactableActor.DisableColliders();
                interactableActor.DisableView();
                interactableActor.SetKinematic(true);
                interactableActor.SetPosition(new Vector3(-100, 0, 0));
            }
            else
            {
                actor.DisableView();
                actor.transform.position = new Vector3(-100, 0, 0);
            }

            Cmd_QueueForRemoval(actor.netId);
        }
    }
}
