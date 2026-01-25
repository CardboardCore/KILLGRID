using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.Players;

namespace KILLGRID.Gameplay.Turns.StateMachines.States
{
    public class TurnActionsState : State
    {
        [Inject] private RoundManager roundManager;

        private PlayerActionComponent playerActionComponent;

        protected override void OnEnter()
        {
            PlayerActor currentPlayer = roundManager.GetCurrentPlayer();

            playerActionComponent = currentPlayer.GetComponent<PlayerActionComponent>();
            playerActionComponent.ServerEndTurnEvent += OnTurnEnded;
            playerActionComponent.TakeTurn();
        }

        protected override void OnExit()
        {
            playerActionComponent.ServerEndTurnEvent -= OnTurnEnded;
            playerActionComponent = null;
        }

        private void OnTurnEnded()
        {
            ToNextState();
        }
    }
}
