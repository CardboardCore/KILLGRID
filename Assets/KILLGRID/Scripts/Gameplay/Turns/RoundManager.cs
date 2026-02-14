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

        public int CurrentRound => currentRound;

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
        public int GetCurrentPlayerIndex()
        {
            return currentPlayer;
        }

        [Server]
        public PlayerActor GetCurrentPlayer()
        {
            return playerManager.GetPlayer(currentPlayer);
        }

        /// <summary>
        /// Gets the opponent player, based on the current player's turn. Assumes a 2-player game.
        /// </summary>
        /// <returns></returns>
        [Server]
        public PlayerActor GetOpponentPlayer()
        {
            return playerManager.GetPlayer((currentPlayer + 1) % playerManager.PlayerCount);
        }
    }
}
