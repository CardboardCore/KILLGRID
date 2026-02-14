using System.Collections.Generic;
using Attic.Utilities;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.Placeables.PlaceableActorComponents;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class UnitAttackState : PlayerActionState
    {
        private PlayerOwnedPlaceablesComponent playerOwnedPlaceablesComponent;

        private readonly List<AttackComponent> awaitingAttackFinishedList = new List<AttackComponent>();

        protected override void OnEnter()
        {
            base.OnEnter();

            playerOwnedPlaceablesComponent = owningStateMachine.Owner.GetComponent<PlayerOwnedPlaceablesComponent>();

            PlaceableActor[] ownedUnits = playerOwnedPlaceablesComponent.GetOwnedUnits();

            awaitingAttackFinishedList.Clear();

            for (int i = 0; i < ownedUnits.Length; i++)
            {
                PlaceableActor ownedUnit = ownedUnits[i];
                AttackComponent attackComponent = ownedUnit.GetComponent<AttackComponent>();

                if (!attackComponent)
                {
                    Log.Warn($"Unit {ownedUnit.name} does not have an AttackComponent and will be skipped in attack phase.");

                    continue;
                }

                attackComponent.AttackFinishedEvent += OnAttackFinished;
                awaitingAttackFinishedList.Add(attackComponent);

                attackComponent.RequestCanAttack(owningStateMachine.Owner, canAttack => {
                    // This logic is called after a delay, due to a server round trip
                    if (!canAttack)
                    {
                        OnAttackFinished(attackComponent);
                        return;
                    }

                    attackComponent.RequestAttack(owningStateMachine.Owner);
                });
            }

            if (awaitingAttackFinishedList.Count == 0)
            {
                Continue();
            }
        }

        protected override void OnExit()
        {
            base.OnExit();

            foreach (AttackComponent attackComponent in awaitingAttackFinishedList)
            {
                attackComponent.AttackFinishedEvent -= OnAttackFinished;
            }
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {

        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {

        }

        private void OnAttackFinished(AttackComponent obj)
        {
            obj.AttackFinishedEvent -= OnAttackFinished;
            awaitingAttackFinishedList.Remove(obj);

            if (awaitingAttackFinishedList.Count == 0)
            {
                Continue();
            }
        }

        private void Continue()
        {
            owningStateMachine.Owner.GetComponent<PlayerCameraComponent>().ResetCameraStep();
            owningStateMachine.Owner.MyMonitor.SetIsPlayerTurn(false);
            owningStateMachine.EndActionPhase();
        }
    }
}
