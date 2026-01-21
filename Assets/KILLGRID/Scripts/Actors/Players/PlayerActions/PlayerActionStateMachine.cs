using Attic.StateMachines;
using KILLGRID.Actors.Players.PlayerActions.States;

namespace KILLGRID.Actors.Players.PlayerActions
{
    public class PlayerActionStateMachine : StateMachine
    {
        public PlayerActor Owner { get; private set; }

        public PlayerActionStateMachine(PlayerActor owner) : base(true)
        {
            Owner = owner;

            SetInitialState<IdleState>();

            AddFreeFlowTransition<IdleState, InsertMemoryBankState>();

            AddFreeFlowTransition<InsertMemoryBankState, IdleState>();
        }
    }
}
