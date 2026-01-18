using Attic.StateMachines;
using KILLGRID.Gameplay.StateMachines.States;

namespace KILLGRID.Gameplay.StateMachines
{
    public class GameplayStateMachine : StateMachine
    {
        public GameplayStateMachine(bool enableDebugging) : base(enableDebugging)
        {
            SetInitialState<LoadPlayerState>();

            AddStaticTransition<LoadPlayerState, SpawnGridState>();
            AddStaticTransition<SpawnGridState, ActiveGameState>();
        }
    }
}
