using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.HexGrid;
using Mirror;

namespace KILLGRID.Gameplay.StateMachines.States
{
    public class SpawnGridState : State
    {
        [Inject] private HexGridActor hexGridActor;

        protected override void OnEnter()
        {
            if (NetworkServer.active)
            {
                hexGridActor.SpawnGrid();
            }

            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
