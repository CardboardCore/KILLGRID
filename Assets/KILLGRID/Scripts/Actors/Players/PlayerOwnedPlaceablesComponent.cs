using System.Collections.Generic;
using System.Linq;
using Attic.Mirror.Actors.Components;
using Attic.Utilities;
using KILLGRID.Actors.Placeables;
using KILLGRID.Gameplay.Placeables;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerOwnedPlaceablesComponent : ActorComponent
    {
        private readonly SyncList<uint> ownedPlaceablesNetIds = new SyncList<uint>();

        public IReadOnlyList<uint> OwnedPlaceablesNetIds => ownedPlaceablesNetIds;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isClient)
            {
                return;
            }

            ownedPlaceablesNetIds.OnAdd += OnOwnedPlaceableAdded;
        }

        protected override void OnReleased()
        {
            if (isClient)
            {
                ownedPlaceablesNetIds.OnAdd -= OnOwnedPlaceableAdded;
            }

            base.OnReleased();
        }

        [Client]
        private void OnOwnedPlaceableAdded(int index)
        {

        }

        [Command(requiresAuthority = false)]
        private void Cmd_AddOwnedPlaceable(uint placeableNetId)
        {
            ownedPlaceablesNetIds.Add(placeableNetId);
        }

        [Command(requiresAuthority = false)]
        public void Cmd_RemoveOwnedPlaceable(uint placeableNetId)
        {
            ownedPlaceablesNetIds.Remove(placeableNetId);
        }

        [Client]
        public PlaceableActor[] GetOwnedPlaceables(params PlaceableType[] types)
        {
            List<PlaceableActor> ownedPlaceables = new List<PlaceableActor>();

            foreach (uint ownedPlaceablesNetId in ownedPlaceablesNetIds)
            {
                if (NetworkClient.spawned.TryGetValue(ownedPlaceablesNetId, out NetworkIdentity identity))
                {
                    PlaceableActor placeableActor = identity.GetComponent<PlaceableActor>();

                    if (!placeableActor)
                    {
                        Log.Warn($"Seems like actor <{identity.name}> is not a placeable actor");
                        continue;
                    }

                    if (types.Contains(placeableActor.PlaceableType))
                    {
                        ownedPlaceables.Add(placeableActor);
                    }
                }
            }

            return ownedPlaceables.ToArray();
        }

        [Client]
        public PlaceableActor[] GetOwnedUnits()
        {
            return GetOwnedPlaceables(PlaceableTypeExtensions.UnitTypes);
        }

        [Client]
        public void RequestAddOwnedPlaceable(PlaceableActor placeableActor)
        {
            Cmd_AddOwnedPlaceable(placeableActor.netId);
        }
    }
}
