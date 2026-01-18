namespace Attic.Mirror.Actors.Interacting
{
    public abstract class InteractRequirement : AtticNetworkBehaviour
    {
        protected Actor Owner { get; private set; }

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        protected virtual void OnInitialize(Actor owner)
        {

        }

        public void Initialize(Actor owner)
        {
            Owner = owner;
            OnInitialize(Owner);
        }

        public abstract bool CanInteract(Actor interactingActor);
    }
}
