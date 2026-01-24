using System;
using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.Players.PlayerActions;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerActionComponent : ActorComponent
    {
        private PlayerActionStateMachine playerActionStateMachine;

        public event Action ServerEndTurnEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isLocalPlayer)
            {
                return;
            }

            playerActionStateMachine = new PlayerActionStateMachine(Owner as PlayerActor);
            playerActionStateMachine.ActionPhaseEndedEvent += OnActionPhaseEnded;
        }

        protected override void OnReleased()
        {
            if (isLocalPlayer)
            {
                playerActionStateMachine.ActionPhaseEndedEvent -= OnActionPhaseEnded;
                playerActionStateMachine.Stop();
                playerActionStateMachine = null;
            }

            base.OnReleased();
        }

        [Client]
        private void OnActionPhaseEnded()
        {
            playerActionStateMachine.Stop();

            Cmd_EndTurn();
        }

        [Command(requiresAuthority = false)]
        private void Cmd_EndTurn()
        {
            if (!isServer)
            {
                return;
            }

            ServerEndTurnEvent?.Invoke();
        }

        [ClientRpc]
        private void Rpc_TakeTurn()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            playerActionStateMachine.Start();
        }

        [Server]
        public void TakeTurn()
        {
            // TODO: Make this target rpc
            Rpc_TakeTurn();
        }
    }
}
