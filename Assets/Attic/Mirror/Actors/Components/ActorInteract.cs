namespace Attic.Mirror.Actors.Components
{
    public abstract class ActorInteract : ActorComponent
    {
        protected ActorCarry ActorCarry;

        protected bool CanFindInteractables { get; private set; }

        protected override void OnInjected()
        {
            base.OnInjected();

            ActorCarry = Owner.GetActorComponent<ActorCarry>();
        }

        protected virtual void Update()
        {
            CanFindInteractables = !ActorCarry || !ActorCarry.IsCarrying;
        }
    }
}
