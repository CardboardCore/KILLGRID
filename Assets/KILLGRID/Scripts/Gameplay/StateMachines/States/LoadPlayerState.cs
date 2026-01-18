using Attic.StateMachines;
using Mirror;

namespace KILLGRID.Gameplay.StateMachines.States
{
    public class LoadPlayerState : State
    {
        protected override void OnEnter()
        {
            // Only load player in this state if we are the server as we needed to spawn the level first
            // Client spawning is handled by the RunNetworkManager
            if (NetworkServer.active && NetworkClient.active)
            {
                NetworkManager.singleton.OnServerAddPlayer(NetworkServer.connections[0]);
            }

            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
