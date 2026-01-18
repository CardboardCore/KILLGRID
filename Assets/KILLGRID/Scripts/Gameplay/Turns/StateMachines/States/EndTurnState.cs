using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Input;

namespace KILLGRID.Gameplay.Turns.StateMachines.States
{
    public class EndTurnState : State
    {
        [Inject] private InputManager inputManager;
        [Inject] private RoundManager roundManager;

        protected override void OnEnter()
        {
            inputManager.Player.Disable();
            roundManager.EndPlayerTurn();

            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
