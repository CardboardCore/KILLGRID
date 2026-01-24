using Attic.StateMachines;
using KILLGRID.Gameplay.StateMachines.States;

namespace KILLGRID.Gameplay.StateMachines
{
    public class GameplayStateMachine : StateMachine
    {
        public GameplayStateMachine(bool enableDebugging) : base(enableDebugging)
        {
            SetInitialState<LoadPlayerState>();

            AddStaticTransition<LoadPlayerState, AwaitFullGameState>();
            AddStaticTransition<AwaitFullGameState, SpawnGridState>();
            AddStaticTransition<SpawnGridState, ActiveGameState>();
        }
    }
}
