using System;
using System.Linq;
using Attic.DI;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Players;
using Mirror;

namespace KILLGRID.Actors.Placeables.PlaceableActorComponents
{
    public class AttackComponent : PlaceableActorComponent
    {
        [Inject] private HexGridActor hexGridActor;

        public event Action<AttackComponent> AttackFinishedEvent;

        // Store callback for async result
        private Action<bool> canAttackCallback;

        [Server]
        protected override void OnServerPlacedInternal()
        {

        }

        [Server]
        protected override void OnServerRemovedInternal()
        {

        }

        [Server]
        protected override void OnServerTurnStartInternal()
        {

        }

        [Server]
        protected override void OnServerTurnEndInternal()
        {

        }

        [Command(requiresAuthority = false)]
        private void CmdRequestCanAttack(PlayerActor playerActor)
        {
            bool result = false;

            if (Owner)
            {
                HexTileActor[] tiles = hexGridActor.GetOppositeEdgeTiles(Owner.OwningPlayerIndex);

                // Check if this placeable is standing on one of the tiles that can attack
                if (Owner.OccupyingTile != null)
                {
                    result = tiles.Contains(Owner.OccupyingTile);
                }
            }

            Rpc_ReceiveCanAttackResult(playerActor.netIdentity.connectionToClient, result);
        }

        [TargetRpc]
        private void Rpc_ReceiveCanAttackResult(NetworkConnectionToClient target, bool result)
        {
            canAttackCallback?.Invoke(result);
            canAttackCallback = null;
        }

        // Client-side entry point: request attack check, provide callback
        [Client]
        public void RequestCanAttack(PlayerActor playerActor, Action<bool> callback)
        {
            if (!isClient)
            {
                throw new InvalidOperationException("RequestCanAttack can only be called on client");
            }

            if (canAttackCallback != null)
            {
                throw new InvalidOperationException("A CanAttack request is already pending");
            }

            canAttackCallback = callback;

            CmdRequestCanAttack(playerActor);
        }

        [Client]
        public void RequestTryAttack()
        {
            AttackFinishedEvent?.Invoke(this);
        }
    }
}
