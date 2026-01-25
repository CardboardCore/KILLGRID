using Attic.StateMachines;
using KILLGRID.Gameplay.Turns.StateMachines.States;

namespace KILLGRID.Gameplay.Turns.StateMachines
{
    /// <summary>
    /// Runs on server only. Manages the flow of a player's turn.
    /// </summary>
    public class TurnStateMachine : StateMachine
    {
        public TurnStateMachine(bool enableDebugging) : base(enableDebugging)
        {
            SetInitialState<SpawnMaxMemoryBanksState>();

            AddStaticTransition<SpawnMaxMemoryBanksState, StartTurnState>();

            AddStaticTransition<RefillMemoryBankState, StartTurnState>();
            AddStaticTransition<StartTurnState, TurnActionsState>();
            AddStaticTransition<TurnActionsState, EndTurnState>();
            AddStaticTransition<EndTurnState, RefillMemoryBankState>();
        }
    }
}
