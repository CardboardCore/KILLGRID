using Attic.DI;
using Attic.StateMachines;
using Attic.Utilities;
using KILLGRID.Networking.Edgegap;
using KILLGRID.Networking.Mirror;

namespace KILLGRID.Application.StateMachines.States
{
    public class CreateSessionState : State
    {
        [Inject] private MirrorRelaySessionController mirrorRelaySessionController;

        protected override void OnEnter()
        {
            CreateSession();
        }

        protected override void OnExit()
        {

        }

        private async void CreateSession()
        {
            ApiResponse response = await mirrorRelaySessionController.CreateSession();

            if (response is { error: not null })
            {
                Log.Error($"{response.error}");
                return;
            }

            Log.Write("Session created successfully.");

            ToState<GameplayState>();
        }
    }
}
