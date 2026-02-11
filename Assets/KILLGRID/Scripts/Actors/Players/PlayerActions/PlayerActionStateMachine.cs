using System;
using Attic.StateMachines;
using KILLGRID.Actors.Players.PlayerActions.States;

namespace KILLGRID.Actors.Players.PlayerActions
{
    /// <summary>
    /// Runs entirely on local client. Manages the flow of a player's action phase.
    /// </summary>
    public class PlayerActionStateMachine : StateMachine
    {
        public PlayerActor Owner { get; }

        public event Action ActionPhaseEndedEvent;

        public PlayerActionStateMachine(PlayerActor owner) : base(true)
        {
            Owner = owner;

            SetInitialState<CheckEnergyState>();

            AddFreeFlowTransition<CheckEnergyState, AwaitingPlayerSelectionState>();
            AddFreeFlowTransition<CheckEnergyState, UnitMovementState>();

            AddFreeFlowTransition<AwaitingPlayerSelectionState, InsertMemoryBankState>();
            AddFreeFlowTransition<AwaitingPlayerSelectionState, UnitMovementState>();

            AddFreeFlowTransition<InsertMemoryBankState, CheckEnergyState>();
            AddFreeFlowTransition<InsertMemoryBankState, AwaitingPlayerSelectionState>();

            AddFreeFlowTransition<UnitMovementState, UnitAttackState>();
        }

        public void EndActionPhase()
        {
            Owner.MyMonitor.SetState("Awaiting Opponent");
            ActionPhaseEndedEvent?.Invoke();
        }
    }
}
