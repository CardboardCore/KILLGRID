using Attic.DI;
using Attic.Mirror;
using KILLGRID.Actors.Players;
using Mirror;
using PrisonBreak.Players;

namespace KILLGRID.Gameplay.Turns
{
    [Injectable]
    public class RoundManager : AtticNetworkBehaviour
    {
        [Inject] private PlayerManager playerManager;

        [SyncVar] private int currentRound = 0;
        [SyncVar] private int currentPlayer = 0;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        public void IncrementRound()
        {
            currentRound++;
        }

        [Server]
        public void EndPlayerTurn()
        {
            currentPlayer = (currentPlayer + 1) % playerManager.PlayerCount;

            if (currentPlayer == 0)
            {
                IncrementRound();
            }
        }

        [Server]
        public PlayerActor GetCurrentPlayer()
        {
            return playerManager.GetPlayer(currentPlayer);
        }
    }
}
