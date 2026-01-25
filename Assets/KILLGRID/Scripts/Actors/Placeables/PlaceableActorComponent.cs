using Attic.Mirror.Actors.Components;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{
    [RequireComponent(typeof(PlaceableActor))]
    public abstract class PlaceableActorComponent : ActorComponent
    {
        protected PlaceableActor Owner { get; private set; }

        protected abstract void OnServerPlacedInternal();
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
