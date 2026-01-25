using Attic.Mirror;
using Attic.Utilities;
using KILLGRID.Actors.TableButtons;
using Mirror;
using UnityEngine;

namespace KILLGRID.Gameplay.Tables
{
    public class TableSpot : AtticNetworkBehaviour
    {
        [SyncVar] private bool isOccupied = false;

        public bool IsOccupied => isOccupied;

        public MemoryBankComponent MemoryBankComponent { get; private set; }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.02f);
            Gizmos.DrawRay(transform.position, transform.right * 0.175f);
            Gizmos.DrawRay(transform.position, -transform.right * 0.175f);
        }

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        public void PlaceMemoryBank(MemoryBankComponent memoryBank)
        {
            if (isOccupied)
            {
                Log.Error("Table spot is already occupied.");
                return;
            }

            memoryBank.transform.position = transform.position;
            memoryBank.transform.rotation = transform.rotation;

            memoryBank.CacheTableSpot(this);

            MemoryBankComponent = memoryBank;

            isOccupied = true;
        }

        [Server]
        public void ClearSpot()
        {
            MemoryBankComponent = null;
            isOccupied = false;
        }
    }
}
