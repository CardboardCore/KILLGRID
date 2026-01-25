using KILLGRID.Actors.Interactables;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class AwaitingPlayerSelectionState : PlayerActionState
    {
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
