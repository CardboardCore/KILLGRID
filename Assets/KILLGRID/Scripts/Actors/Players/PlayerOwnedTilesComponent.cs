using System.Collections.Generic;
using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Placeables;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerOwnedTilesComponent : ActorComponent
    {
        private readonly SyncList<uint> ownedTileNetIds = new SyncList<uint>();

        public IReadOnlyList<uint> OwnedTileNetIds => ownedTileNetIds;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isClient)
            {
                return;
            }

            ownedTileNetIds.OnAdd += OnOwnedTileAdded;
        }

        protected override void OnReleased()
        {
            if (isClient)
            {
                ownedTileNetIds.OnAdd -= OnOwnedTileAdded;
            }

            base.OnReleased();
        }

        [Client]
        private void OnOwnedTileAdded(int index)
        {

        }

        [Server]
        private HexTileActor GetTileActorOnServer(uint actorNetId)
        {
            if (NetworkServer.spawned.TryGetValue(actorNetId, out NetworkIdentity identity))
            {
                return identity.GetComponent<HexTileActor>();
            }

            return null;
        }

        [Command(requiresAuthority = false)]
        private void Cmd_AddOwnedTile(uint hexTileActorNetId)
        {
            ownedTileNetIds.Add(hexTileActorNetId);

            HexTileActor hexTileActor = GetTileActorOnServer(hexTileActorNetId);
            hexTileActor.SetOwner((Owner as PlayerActor).PlayerIndex);
        }

        [Client]
        public void RequestAddOwnedTile(HexTileActor hexTileActor)
        {
            Cmd_AddOwnedTile(hexTileActor.netId);
        }

        [Server]
        public void OnTurnStart()
        {
            // TODO: Do a little delay between each tile's turn start so we can play animations sequentially
            foreach (uint ownedTileNetId in ownedTileNetIds)
            {
                HexTileActor hexTileActor = GetTileActorOnServer(ownedTileNetId);
                hexTileActor.OnTurnStart();
            }
        }

        [Server]
        public void OnTurnEnd()
        {
            foreach (uint ownedTileNetId in ownedTileNetIds)
            {
                HexTileActor hexTileActor = GetTileActorOnServer(ownedTileNetId);
                hexTileActor.OnTurnEnd();
            }
        }

        [Client]
        public T[] GetAllOwnedTilesWithPlaceableComponent<T>() where T : PlaceableActorComponent
        {
            List<T> components = new List<T>();

            foreach (uint ownedTileNetId in ownedTileNetIds)
            {
                if (!NetworkClient.spawned.TryGetValue(ownedTileNetId, out NetworkIdentity identity))
                {
                    continue;
                }

                HexTileActor hexTileActor = identity.GetComponent<HexTileActor>();

                if (!hexTileActor.TryGetPlacedActor(out PlaceableActor placeableActor))
                {
                    continue;
                }

                T component = placeableActor.GetComponent<T>();

                if (component != null)
                {
                    components.Add(component);
                }
            }

            return components.ToArray();
        }
    }
}
