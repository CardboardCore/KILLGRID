using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.Players;
using KILLGRID.Actors.TableButtons;
using KILLGRID.Gameplay.MemoryBanks;
using KILLGRID.Gameplay.Tables;

namespace KILLGRID.Gameplay.Turns.StateMachines.States
{
    public class RefillMemoryBankState : State
    {
        [Inject] private RoundManager roundManager;
        [Inject] private MemoryBankFactory memoryBankFactory;
        [Inject] private Table table;

        protected override void OnEnter()
        {
            PlayerActor currentPlayer = roundManager.GetCurrentPlayer();

            PlayerMemoryBankComponent playerMemoryBankComponent = currentPlayer.GetComponent<PlayerMemoryBankComponent>();
            int maxMemoryBanks = playerMemoryBankComponent.MaxMemoryBanks;
            int currentMemoryBanks = playerMemoryBankComponent.CurrentMemoryBanks;

            int banksToRefill = maxMemoryBanks - currentMemoryBanks;

            MemoryBankTableSpots memoryBankTableSpots = table.GetTableSpotsForPlayer(roundManager.GetCurrentPlayerIndex());

            for (int i = 0; i < banksToRefill; i++)
            {
                MemoryBankType memoryBankType = MemoryBankTypeExtensions.GetRandomNonCoreType();
                MemoryBankComponent memoryBankComponent = memoryBankFactory.Spawn(memoryBankType);

                // Place on the table
                memoryBankTableSpots.PlaceMemoryBankAtRandomFreeSpot(memoryBankComponent);

                // Register owner of memory banks
                playerMemoryBankComponent.AddMemoryBank(memoryBankComponent);
            }

            ToNextState();
        }

        protected override void OnExit()
        {

        }
    }
}
