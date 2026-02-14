using System;
using System.Linq;
using Attic.DI;
using DG.Tweening;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Players;
using KILLGRID.Gameplay.Turns;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables.PlaceableActorComponents
{
    [Serializable]
    public class AttackConfig
    {
        [SerializeField] private AttackComponent.AttackType attackType;
        [SerializeField] private int attackDamage = 1;

        public AttackComponent.AttackType AttackType => attackType;
        public int AttackDamage => attackDamage;
    }

    public class AttackComponent : PlaceableActorComponent
    {
        public enum AttackType
        {
            /// <summary>
            /// Shoot a projectile at the player.
            /// </summary>
            Shoot,

            /// <summary>
            /// Jump off the grid, into the player's face. Destroys the placeable in the process.
            /// </summary>
            JumpSuicide
        }

        [Inject] private HexGridActor hexGridActor;
        [Inject] private RoundManager roundManager;

        [SerializeField] private AttackConfig attackConfig;

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
        private void Cmd_RequestCanAttack(PlayerActor playerActor)
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

        [Command(requiresAuthority = false)]
        private void Cmd_Attack(PlayerActor playerActor)
        {
            PlayerActor opponentPlayerActor = roundManager.GetOpponentPlayer();
            PlayerHealthComponent playerHealthComponent = opponentPlayerActor.GetComponent<PlayerHealthComponent>();

            switch (attackConfig.AttackType)
            {
                case AttackType.Shoot:
                    // TODO: Spawn projectile, on impact do damage and then call Rpc_AttackFinished
                    playerHealthComponent.TakeDamage(attackConfig.AttackDamage);
                    Rpc_AttackFinished(playerActor.connectionToClient);
                    break;

                case AttackType.JumpSuicide:
                    JumpToPlayer(opponentPlayerActor, () => {
                        playerHealthComponent.TakeDamage(attackConfig.AttackDamage);
                        Rpc_AttackFinished(playerActor.connectionToClient);

                        Owner.MarkForDestruction();
                    });
                    break;
            }
        }

        [Server]
        private void JumpToPlayer(PlayerActor playerActor, Action callback)
        {
            Owner.OccupyingTile.Cmd_RemoveActor();

            // Animate this transform to the player's position, then call the callback and destroy this placeable
            Vector3 targetPosition = playerActor.transform.position;

            // Tween to target position with an arc (jump) and then call the callback and destroy this placeable
            transform.DOJump(targetPosition, 1f, 1, 0.5f).SetEase(Ease.OutQuad).OnComplete(() => {
                callback();
            });
        }

        [TargetRpc]
        private void Rpc_AttackFinished(NetworkConnectionToClient target)
        {
            AttackFinishedEvent?.Invoke(this);
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

            Cmd_RequestCanAttack(playerActor);
        }

        [Client]
        public void RequestAttack(PlayerActor playerActor)
        {
            Cmd_Attack(playerActor);
        }
    }
}
