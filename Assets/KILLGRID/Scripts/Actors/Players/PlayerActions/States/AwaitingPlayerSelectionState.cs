using Attic.DI;
using KILLGRID.Actors.Interactables;
using KILLGRID.Input;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class AwaitingPlayerSelectionState : PlayerActionState
    {
        [Inject] private InputManager inputManager;

        protected override void OnEnter()
        {
            base.OnEnter();

            inputManager.Player.Enable();

            owningStateMachine.Owner.MyMonitor.SetState("Awaiting Selection");
            owningStateMachine.Owner.MyMonitor.PlayerEndTurnPressedEvent += PlayerEndTurnPressed;
        }

        protected override void OnExit()
        {
            inputManager.Player.Disable();
            owningStateMachine.Owner.MyMonitor.PlayerEndTurnPressedEvent -= PlayerEndTurnPressed;

            base.OnExit();
        }

        private void PlayerEndTurnPressed()
        {
            ToState<UnitMovementState>();
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {

        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {
            switch (interactableComponent.InteractableConfig.InteractableType)
            {
                case InteractableType.MemoryBank:
                    ToState<InsertMemoryBankState>();
                    break;

                case InteractableType.Tile:
                    break;
            }
        }
    }
}
