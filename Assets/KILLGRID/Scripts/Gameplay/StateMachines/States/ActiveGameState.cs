using Attic.DI;
using Attic.StateMachines;
using Attic.Utils.Invoking;
using KILLGRID.Gameplay.Turns.StateMachines;
using Mirror;

namespace KILLGRID.Gameplay.StateMachines.States
{
    public class ActiveGameState : State
    {
        [Inject] private InvokeWrapper invokeWrapper;

        private TurnStateMachine turnStateMachine;

        protected override void OnEnter()
        {
            if (NetworkServer.active)
            {
                invokeWrapper.Invoke(() => {
                    turnStateMachine = new TurnStateMachine(true);
                    turnStateMachine.Start();
                }, 2f);
            }
        }

        protected override void OnExit()
        {
            if (turnStateMachine != null && NetworkServer.active)
            {
                turnStateMachine.Stop();
                turnStateMachine = null;
            }
        }
    }
}
