using Attic.DI;
using Attic.StateMachines;
using PrisonBreak.Players;

namespace KILLGRID.Gameplay.StateMachines.States
{
    public class AwaitFullGameState : State
    {
        [Inject] private PlayerManager playerManager;

        protected override void OnEnter()
        {
            if (!playerManager.AwaitFullGame)
            {
                ToNextState();
                return;
            }

            if (playerManager.PlayerCount < 2)
            {
                playerManager.PlayerAddedEvent += OnPlayerAdded;
                return;
            }

            ToNextState();
        }

        protected override void OnExit()
        {
            playerManager.PlayerAddedEvent -= OnPlayerAdded;
        }

        private void OnPlayerAdded(PlayerEntry obj)
        {
            if (playerManager.PlayerCount < 2)
            {
                return;
            }

            ToNextState();
        }
    }
}
