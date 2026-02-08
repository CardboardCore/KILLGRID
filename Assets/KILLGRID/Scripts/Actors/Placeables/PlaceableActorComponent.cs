using Attic.Mirror.Actors.Components;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{
    [RequireComponent(typeof(PlaceableActor))]
    public abstract class PlaceableActorComponent : ActorComponent
    {
        protected new PlaceableActor Owner { get; private set; }

        protected abstract void OnServerPlacedInternal();
        protected abstract void OnServerRemovedInternal();
        protected abstract void OnServerTurnStartInternal();
        protected abstract void OnServerTurnEndInternal();

        public void Initialize(PlaceableActor owner)
        {
            Owner = owner;
        }

        [Server]
        public void OnPlaced()
        {
            OnServerPlacedInternal();
        }

        [Server]
        public void OnRemoved()
        {
            OnServerRemovedInternal();
        }

        [Server]
        public void OnTurnStart()
        {
            OnServerTurnStartInternal();
        }

        [Server]
        public void OnTurnEnd()
        {
            OnServerTurnEndInternal();
        }
    }
}
