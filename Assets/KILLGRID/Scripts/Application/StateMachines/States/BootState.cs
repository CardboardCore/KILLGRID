using Attic.DI;
using Attic.StateMachines;
using Attic.Utils.Invoking;

namespace KILLGRID.Application.StateMachines.States
{
    public class BootState : State
    {
        [Inject] private InvokeWrapper invokeWrapper;

        protected override void OnEnter()
        {
            invokeWrapper.Invoke(ToNextState, 2f);
        }

        protected override void OnExit()
        {

        }
    }
}
