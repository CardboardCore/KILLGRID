using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Networking.Edgegap;
using KILLGRID.Networking.Mirror;

namespace KILLGRID.Application.StateMachines.States
{
    public class FindSessionState : State
    {
        [Inject] private MirrorRelaySessionController mirrorRelaySessionController;

        protected override void OnEnter()
        {
            FindSession();
        }

        protected override void OnExit()
        {

        }

        private async void FindSession()
        {
            ApiResponse response = await mirrorRelaySessionController.FindSession();

            if (response.error != null)
            {
                // if (response.error.Equals("No Edgegap Relay Session found."))
                // {
                    ToState<CreateSessionState>();
                    return;
                // }
            }

            // Session id is cached in the RelaySessionController

            ToState<JoinSessionState>();
        }
    }
}
