using Attic.DI;

using Mirror;

namespace Attic.Mirror
{
    public abstract class AtticNetworkBehaviour : NetworkBehaviour
    {
        protected enum InjectTiming
        {
            /// <summary>
            /// On Mirror "Start Server"
            /// </summary>
            Server,

            /// <summary>
            /// On Mirror "Start Client"
            /// </summary>
            Client,

            /// <summary>
            /// On Mirror "Start Local Player" only
            /// </summary>
            Local
        }

        /// <summary>
        /// The timing of dependency injection for this NetworkBehaviour
        /// </summary>
        protected virtual InjectTiming MyInjectTiming => InjectTiming.Client;

        protected abstract void OnInjected();
        protected abstract void OnReleased();

        protected bool IsReady { get; private set; }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (MyInjectTiming == InjectTiming.Server)
            {
                Injector.Inject(this);
                OnInjected();

                IsReady = true;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (MyInjectTiming == InjectTiming.Client)
            {
                Injector.Inject(this);
                OnInjected();

                IsReady = true;
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (MyInjectTiming == InjectTiming.Local)
            {
                Injector.Inject(this);
                OnInjected();

                IsReady = true;
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (MyInjectTiming == InjectTiming.Server)
            {
                OnReleased();
                Injector.Release(this);

                IsReady = false;
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (MyInjectTiming == InjectTiming.Client)
            {
                OnReleased();
                Injector.Release(this);

                IsReady = false;
            }
        }

        public override void OnStopLocalPlayer()
        {
            base.OnStopLocalPlayer();

            if (MyInjectTiming == InjectTiming.Local)
            {
                OnReleased();
                Injector.Release(this);

                IsReady = false;
            }
        }
    }
}
