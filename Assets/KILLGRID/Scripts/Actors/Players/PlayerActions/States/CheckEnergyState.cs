using Attic.DI;
using Attic.Utilities;
using Attic.Utils.Invoking;
using KILLGRID.Actors.Interactables;
using KILLGRID.Gameplay.Turns;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class CheckEnergyState : PlayerActionState
    {
        [Inject] private InvokeWrapper invokeWrapper;
        [Inject] private RoundManager roundManager;

        private PlayerEnergyComponent playerEnergyComponent;

        protected override void OnEnter()
        {
            base.OnEnter();

            // Every time a player gets a turn or finishes an action, they go back to idle state
            playerEnergyComponent = owningStateMachine.Owner.GetComponent<PlayerEnergyComponent>();
            playerEnergyComponent.EnergyUpdatedEvent += OnEnergyUpdated;

            owningStateMachine.Owner.MyMonitor.SetIsPlayerTurn(true);
            owningStateMachine.Owner.MyMonitor.SetState("Checking Energy");
            owningStateMachine.Owner.MyMonitor.SetRoundNumber(roundManager.CurrentRound);

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

            owningStateMachine.Owner.MyMonitor.SetPlayerEnergy(totalEnergy);

            invokeWrapper.Invoke(() => {
                // Check if a memory bank can be inserted based on energy
                PlayerMemoryBankComponent playerMemoryBankComponent = owningStateMachine.Owner.GetComponent<PlayerMemoryBankComponent>();
                bool canPlaceAnyMemoryBank = playerMemoryBankComponent.CanPlaceAnyMemoryBank(totalEnergy);

                if (!canPlaceAnyMemoryBank)
                {
                    EndActionPhase();
                    return;
                }

                // Else, go to awaiting player selection state
                ToState<AwaitingPlayerSelectionState>();
            }, 1f);
        }

        private void EndActionPhase()
        {
            owningStateMachine.Owner.GetComponent<PlayerCameraComponent>().ResetCameraStep();
            owningStateMachine.Owner.MyMonitor.SetIsPlayerTurn(false);
            owningStateMachine.EndActionPhase();
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {

        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {

        }
    }
}
