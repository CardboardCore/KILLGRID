using System.Collections.Generic;
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

        private readonly List<MovementComponent> awaitingMovementFinishedList = new List<MovementComponent>();

        protected override void OnEnter()
        {
            base.OnEnter();

            playerOwnedPlaceablesComponent = owningStateMachine.Owner.GetComponent<PlayerOwnedPlaceablesComponent>();

            PlaceableActor[] ownedUnits = playerOwnedPlaceablesComponent.GetOwnedUnits();

            awaitingMovementFinishedList.Clear();

            for (int i = 0; i < ownedUnits.Length; i++)
            {
                PlaceableActor ownedUnit = ownedUnits[i];
                MovementComponent movementComponent = ownedUnit.GetComponent<MovementComponent>();

                if (!movementComponent)
                {
                    Log.Warn($"Unit {ownedUnit.name} does not have a MovementComponent and will be skipped in movement phase.");

                    continue;
                }

                movementComponent.MovementFinishedEvent += OnMovementFinished;
                awaitingMovementFinishedList.Add(movementComponent);

                invokeWrapper.Invoke(movementComponent.RequestTryMove, 1 * i);
            }

            if (awaitingMovementFinishedList.Count == 0)
            {
                Continue();
                return;
            }

            owningStateMachine.Owner.GetComponent<PlayerCameraComponent>().SetMaxStep();
        }

        protected override void OnExit()
        {
            base.OnExit();

            foreach (MovementComponent movementComponent in awaitingMovementFinishedList)
            {
                movementComponent.MovementFinishedEvent -= OnMovementFinished;
            }
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {

        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {

        }

        private void OnMovementFinished(MovementComponent movementComponent)
        {
            movementComponent.MovementFinishedEvent -= OnMovementFinished;
            awaitingMovementFinishedList.Remove(movementComponent);

            if (awaitingMovementFinishedList.Count == 0)
            {
                Continue();
            }
        }

        private void Continue()
        {
            invokeWrapper.Invoke(ToState<UnitAttackState>, 1f);
        }
    }
}
