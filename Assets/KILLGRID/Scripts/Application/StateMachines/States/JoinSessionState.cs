using Attic.DI;
using Attic.StateMachines;
using Attic.Utilities;
using KILLGRID.Networking.Mirror;

namespace KILLGRID.Application.StateMachines.States
{
    public class JoinSessionState : State
    {
        [Inject] private MirrorRelaySessionController mirrorRelaySessionController;
        [Inject] private GameNetworkManager gameNetworkManager;

        protected override void OnEnter()
        {
            gameNetworkManager.ClientConnectedToServerEvent += OnClientConnectedToServer;
            JoinSession();
        }

        protected override void OnExit()
        {
            gameNetworkManager.ClientConnectedToServerEvent -= OnClientConnectedToServer;
        }

        private async void JoinSession()
        {
            try
            {
                await mirrorRelaySessionController.JoinSession(mirrorRelaySessionController.SessionId);
                Log.Write($"Successfully joined session with ID: {mirrorRelaySessionController.SessionId}");
            }
            catch (System.Exception e)
            {
                Log.Error($"Failed to join session: {e.Message}");
                // Handle error, maybe transition to an error state or retry
            }
        }

        private void OnClientConnectedToServer(bool success)
        {
            Log.Write($"Connected to server: {success}");

            if (success)
            {
                ToState<GameplayState>();
            }
            else
            {
                mirrorRelaySessionController.AddCurrentSessionIdToIgnoreList();
                mirrorRelaySessionController.ClearSessionId();

                mirrorRelaySessionController.Disconnect(ToState<FindSessionState>);
            }
        }
    }
}
