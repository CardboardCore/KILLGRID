using Attic.StateMachines;
using KILLGRID.Actors.Interactables;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public abstract class PlayerActionState : State<PlayerActionStateMachine>
    {
        protected override void OnEnter()
        {
            owningStateMachine.Owner.PlayerInteractComponent.HoverEvent += OnHoverInteractable;
            owningStateMachine.Owner.PlayerInteractComponent.SelectEvent += OnSelectInteractable;
        }

        protected override void OnExit()
        {
            owningStateMachine.Owner.PlayerInteractComponent.HoverEvent -= OnHoverInteractable;
            owningStateMachine.Owner.PlayerInteractComponent.SelectEvent -= OnSelectInteractable;
        }

        private void OnHoverInteractable(InteractableComponent interactableComponent)
        {
            OnHover(interactableComponent);
        }

        private void OnSelectInteractable(InteractableComponent interactableComponent)
        {
            OnSelect(interactableComponent);
        }

        protected abstract void OnHover(InteractableComponent interactableComponent);
        protected abstract void OnSelect(InteractableComponent interactableComponent);
    }
}
