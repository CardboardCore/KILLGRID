using Attic.StateMachines;
using KILLGRID.Gameplay.StateMachines;

namespace KILLGRID.Application.StateMachines.States
{
    public class GameplayState : State
    {
        private GameplayStateMachine gameplayStateMachine;

        protected override void OnEnter()
        {
            gameplayStateMachine = new GameplayStateMachine(true);
            gameplayStateMachine.StoppedEvent += OnGameplayStateMachineStopped;

            gameplayStateMachine.Start();
        }

        protected override void OnExit()
        {
            gameplayStateMachine.StoppedEvent -= OnGameplayStateMachineStopped;
        }

        private void OnGameplayStateMachineStopped()
        {
            // TODO: Back to main menu or exit the game
        }
    }
}
