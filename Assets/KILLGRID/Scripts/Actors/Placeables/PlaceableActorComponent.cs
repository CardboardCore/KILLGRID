using Attic.Mirror.Actors.Components;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{
    [RequireComponent(typeof(PlaceableActor))]
    public abstract class PlaceableActorComponent : ActorComponent
    {
        protected PlaceableActor Owner { get; private set; }

        protected abstract void OnServerTurnStart();

        public void Initialize(PlaceableActor owner)
        {
            Owner = owner;
        }

        [Server]
        public void OnTurnStart()
        {
            OnServerTurnStart();
        }
    }
}
