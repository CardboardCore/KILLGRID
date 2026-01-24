using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.Players;
using KILLGRID.Input;

namespace KILLGRID.Gameplay.Turns.StateMachines.States
{
    public class EndTurnState : State
    {
        [Inject] private InputManager inputManager;
        [Inject] private RoundManager roundManager;

        protected override void OnEnter()
        {
            PlayerActor currentPlayer = roundManager.GetCurrentPlayer();

            currentPlayer.GetComponent<PlayerOwnedTilesComponent>().OnTurnEnd();
            currentPlayer.DisableInput();

            roundManager.EndPlayerTurn();

            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
