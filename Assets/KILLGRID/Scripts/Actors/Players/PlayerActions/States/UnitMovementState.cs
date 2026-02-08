using Attic.Cameras;
using Attic.Cameras.Transitions;
using Attic.Cameras.VirtualCameras;
using Attic.DI;
using Attic.Utilities;
using Attic.Utils.Invoking;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.Placeables.PlaceableActorComponents;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class UnitMovementState : PlayerActionState
    {
        [Inject] private InvokeWrapper invokeWrapper;

        private PlayerOwnedPlaceablesComponent playerOwnedPlaceablesComponent;

        protected override void OnEnter()
        {
            base.OnEnter();

            owningStateMachine.Owner.GetComponent<PlayerCameraComponent>().SetMaxStep();

            playerOwnedPlaceablesComponent = owningStateMachine.Owner.GetComponent<PlayerOwnedPlaceablesComponent>();

            PlaceableActor[] ownedUnits = playerOwnedPlaceablesComponent.GetOwnedUnits();

            for (int i = 0; i < ownedUnits.Length; i++)
            {
                PlaceableActor ownedUnit = ownedUnits[i];
                MovementComponent movementComponent = ownedUnit.GetComponent<MovementComponent>();

                if (!movementComponent)
                {
                    Log.Warn($"Unit {ownedUnit.name} does not have a MovementComponent and will be skipped in movement phase.");

                    continue;
                }

                invokeWrapper.Invoke(movementComponent.RequestTryMove, 1 * i);
            }

            invokeWrapper.Invoke(Continue, ownedUnits.Length * 2);
        }

        protected override void OnExit()
        {
            base.OnExit();
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {

        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {

        }

        private void Continue()
        {
            owningStateMachine.Owner.GetComponent<PlayerCameraComponent>().ResetCameraStep();
            owningStateMachine.Owner.MyMonitor.SetIsPlayerTurn(false);
            owningStateMachine.EndActionPhase();
        }
    }
}
