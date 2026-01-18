using Attic.StateMachines;
using KILLGRID.Gameplay.Turns.StateMachines.States;

namespace KILLGRID.Gameplay.Turns.StateMachines
{
    public class TurnStateMachine : StateMachine
    {
        public TurnStateMachine(bool enableDebugging) : base(enableDebugging)
        {
            SetInitialState<StartTurnState>();

            AddStaticTransition<StartTurnState, TurnActionsState>();
            AddStaticTransition<TurnActionsState, EndTurnState>();

            AddStaticTransition<EndTurnState, StartTurnState>();
        }
    }
}
