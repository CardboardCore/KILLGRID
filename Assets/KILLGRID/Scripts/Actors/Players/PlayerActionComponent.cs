using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.Players.PlayerActions;

namespace KILLGRID.Actors.Players
{
    public class PlayerActionComponent : ActorComponent
    {
        private PlayerActionStateMachine playerActionStateMachine;

        protected override void OnInjected()
        {
            base.OnInjected();

            playerActionStateMachine = new PlayerActionStateMachine(Owner as PlayerActor);
            playerActionStateMachine.Start();
        }
    }
}
