using System.Collections.Generic;
using Attic.Mirror;
using Attic.Utilities;
using KILLGRID.Actors.TableButtons;
using Mirror;
using UnityEngine;

namespace KILLGRID.Gameplay.Tables
{
    public class MemoryBankTableSpots : AtticNetworkBehaviour
    {
        [SerializeField] private TableSpot[] tableSpots;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        private bool TryGetFreeRandomSpot(out TableSpot freeSpot)
        {
            // Shuffle table spots and find a free one
            List<TableSpot> shuffledSpots = new List<TableSpot>(tableSpots);

            for (int i = 0; i < shuffledSpots.Count; i++)
            {
                TableSpot temp = shuffledSpots[i];
                int randomIndex = Random.Range(i, shuffledSpots.Count);
                shuffledSpots[i] = shuffledSpots[randomIndex];
                shuffledSpots[randomIndex] = temp;
            }

            foreach (TableSpot spot in shuffledSpots)
            {
                if (spot.IsOccupied)
                {
                    continue;
                }

                freeSpot = spot;
                return true;
            }

            freeSpot = null;
            return false;
        }

        [Server]
        public void PlaceMemoryBankAtRandomFreeSpot(MemoryBankComponent memoryBank)
        {
            if (TryGetFreeRandomSpot(out TableSpot freeSpot))
            {
                freeSpot.PlaceMemoryBank(memoryBank);
            }
            else
            {
                Log.Error("No free table spots available to place the memory bank.");
            }
        }
    }
}
