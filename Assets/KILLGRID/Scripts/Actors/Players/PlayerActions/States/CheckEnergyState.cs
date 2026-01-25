using Attic.DI;
using Attic.Utilities;
using Attic.Utils.Invoking;
using KILLGRID.Actors.Interactables;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class CheckEnergyState : PlayerActionState
    {
        [Inject] private InvokeWrapper invokeWrapper;

        private PlayerEnergyComponent playerEnergyComponent;

        protected override void OnEnter()
        {
            base.OnEnter();

            // Every time a player gets a turn or finishes an action, they go back to idle state
            playerEnergyComponent = owningStateMachine.Owner.GetComponent<PlayerEnergyComponent>();
            playerEnergyComponent.EnergyUpdatedEvent += OnEnergyUpdated;

            invokeWrapper.Invoke(playerEnergyComponent.RequestUpdateEnergy, 1f);
        }

        protected override void OnExit()
        {
            playerEnergyComponent.EnergyUpdatedEvent -= OnEnergyUpdated;
            playerEnergyComponent = null;

            base.OnExit();
        }

        private void OnEnergyUpdated(int totalEnergy)
        {
            Log.Write($"Energy updated: {totalEnergy}");

            invokeWrapper.Invoke(() => {
                // Check if a memory bank can be inserted based on energy
                PlayerMemoryBankComponent playerMemoryBankComponent = owningStateMachine.Owner.GetComponent<PlayerMemoryBankComponent>();
                bool canPlaceAnyMemoryBank = playerMemoryBankComponent.CanPlaceAnyMemoryBank(totalEnergy);

                if (!canPlaceAnyMemoryBank)
                {
                    owningStateMachine.EndActionPhase();
                    return;
                }

                // Check if a facility can be used based on energy

                // If no actions can be done, end this player's turn automatically

                // Else, go to awaiting player selection state
                ToState<AwaitingPlayerSelectionState>();
            }, 1f);
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {

        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {

        }
    }
}
