using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.Players;

namespace KILLGRID.Actors.Interactables
{
    public abstract class InteractRequirement : ActorComponent
    {
        public abstract bool CanInteract(PlayerActor interactingPlayer);
    }
}
