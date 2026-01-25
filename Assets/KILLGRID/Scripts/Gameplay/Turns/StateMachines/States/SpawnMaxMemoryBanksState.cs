using System.Collections.Generic;
using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Actors.Players;
using KILLGRID.Actors.TableButtons;
using KILLGRID.Gameplay.MemoryBanks;
using KILLGRID.Gameplay.Tables;
using Mirror;
using PrisonBreak.Players;

namespace KILLGRID.Gameplay.Turns.StateMachines.States
{
    public class SpawnMaxMemoryBanksState : State
    {
        [Inject] private PlayerManager playerManager;
        [Inject] private MemoryBankFactory memoryBankFactory;
        [Inject] private Table table;

        protected override void OnEnter()
        {
            if (NetworkServer.active)
            {
                List<PlayerActor> players = playerManager.GetAllPlayers();

                for (int i = 0; i < players.Count; i++)
                {
                    PlayerMemoryBankComponent playerMemoryBankComponent = players[i].GetComponent<PlayerMemoryBankComponent>();
                    int maxMemoryBanks = playerMemoryBankComponent.MaxMemoryBanks;

                    MemoryBankTableSpots memoryBankTableSpots = table.GetTableSpotsForPlayer(i);

                    for (int k = 0; k < maxMemoryBanks; k++)
                    {
                        MemoryBankType memoryBankType = k == 0 ? MemoryBankType.CoreHQ : MemoryBankTypeExtensions.GetRandomNonCoreType();
                        MemoryBankComponent memoryBankComponent = memoryBankFactory.Spawn(memoryBankType);

                        // Place on the table
                        memoryBankTableSpots.PlaceMemoryBankAtRandomFreeSpot(memoryBankComponent);

                        // Register owner of memory banks
                        playerMemoryBankComponent.AddMemoryBank(memoryBankComponent);
                    }
                }

                ToNextState();
            }
        }

        protected override void OnExit()
        {

        }
    }
}
