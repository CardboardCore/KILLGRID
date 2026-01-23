using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.Players;
using KILLGRID.Input;
using Mirror;

namespace KILLGRID.Gameplay.Turns.StateMachines.States
{
    public class StartTurnState : State
    {
        [Inject] private InputManager inputManager;
        [Inject] private RoundManager roundManager;

        protected override void OnEnter()
        {
            if (NetworkServer.active)
            {
                PlayerActor currentPlayer = roundManager.GetCurrentPlayer();

                currentPlayer.GetComponent<PlayerOwnedTilesComponent>().OnTurnStart();
                currentPlayer.EnableInput();
            }

            // Reset troops, resources, etc. for the new turn
            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
