using Attic.StateMachines;
using KILLGRID.Application.StateMachines.States;


namespace KILLGRID.Application.StateMachines
{
    public class ApplicationStateMachine : StateMachine
    {
        public ApplicationStateMachine(bool enableDebugging) : base(enableDebugging)
        {
            SetInitialState<BootState>();

            AddStaticTransition<BootState, FindSessionState>();

            AddFreeFlowTransition<FindSessionState, CreateSessionState>();
            AddFreeFlowTransition<FindSessionState, JoinSessionState>();

            AddFreeFlowTransition<CreateSessionState, GameplayState>();

            AddFreeFlowTransition<JoinSessionState, FindSessionState>();
            AddFreeFlowTransition<JoinSessionState, GameplayState>();
        }
    }
}
