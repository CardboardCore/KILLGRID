using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.HexGrid;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerOwnedTilesComponent : ActorComponent
    {
        private SyncList<uint> ownedTileNetIds = new SyncList<uint>();

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

        private void OnOwnedTileAdded(int index)
        {

        }

        [Client]
        private HexTileActor GetTileActorOnClient(uint actorNetId)
        {
            if (NetworkClient.spawned.TryGetValue(actorNetId, out NetworkIdentity identity))
            {
                return identity.GetComponent<HexTileActor>();
            }

            return null;
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
        public void AddOwnedTile(HexTileActor hexTileActor)
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
    }
}
