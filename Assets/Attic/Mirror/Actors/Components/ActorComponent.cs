namespace Attic.Mirror.Actors.Components
{
    public abstract class ActorComponent : AtticNetworkBehaviour
    {
        public Actor Owner { get; private set; }

        protected override void OnInjected()
        {
            Owner = GetComponentInParent<Actor>();
        }

        protected override void OnReleased()
        {

        }

        public virtual void Cleanup()
        {

        }
    }
}
