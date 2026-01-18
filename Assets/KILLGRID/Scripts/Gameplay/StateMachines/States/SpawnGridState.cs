using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.HexGrid;

namespace KILLGRID.Gameplay.StateMachines.States
{
    public class SpawnGridState : State
    {
        [Inject] private HexGridActor hexGridActor;

        protected override void OnEnter()
        {
            hexGridActor.SpawnGrid();
            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
