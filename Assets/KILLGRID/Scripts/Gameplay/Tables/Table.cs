using Attic.DI;
using Attic.Mirror;
using Mirror;
using UnityEngine;

namespace KILLGRID.Gameplay.Tables
{
    [Injectable]
    public class Table : AtticNetworkBehaviour
    {
        [SerializeField] private MemoryBankTableSpots playerOneTableSpots;
        [SerializeField] private MemoryBankTableSpots playerTwoTableSpots;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        public MemoryBankTableSpots GetTableSpotsForPlayer(int playerIndex)
        {
            return playerIndex == 0 ? playerOneTableSpots : playerTwoTableSpots;
        }
    }
}
