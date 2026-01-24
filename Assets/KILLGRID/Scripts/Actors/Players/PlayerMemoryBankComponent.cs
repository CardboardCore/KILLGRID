using Attic.Mirror.Actors.Components;
using Attic.Utilities;
using KILLGRID.Actors.TableButtons;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Players
{
    public class PlayerMemoryBankComponent : ActorComponent
    {
        [SerializeField] private int maxMemoryBanks = 3;

        private SyncList<uint> memoryBankNetIds = new SyncList<uint>();

        public int MaxMemoryBanks => maxMemoryBanks;
        public int CurrentMemoryBanks => memoryBankNetIds.Count;

        [Client]
        public bool IsOwnedByPlayer(uint memoryBankNetId)
        {
            return memoryBankNetIds.Contains(memoryBankNetId);
        }

        [Server]
        public void AddMemoryBank(MemoryBankComponent memoryBank)
        {
            uint memoryBankNetId = memoryBank.netId;

            if (!memoryBankNetIds.Contains(memoryBankNetId) && memoryBankNetIds.Count < maxMemoryBanks)
            {
                memoryBankNetIds.Add(memoryBankNetId);
            }
        }

        [Command(requiresAuthority = false)]
        public void Cmd_RemoveMemoryBank(uint memoryBankNetId)
        {
            if (!memoryBankNetIds.Contains(memoryBankNetId))
            {
                Log.Error($"Trying to remove memory bank {memoryBankNetId} that is not owned by player {Owner.netId}");
                return;
            }

            memoryBankNetIds.Remove(memoryBankNetId);
        }

        [Client]
        public bool CanPlaceAnyMemoryBank(int totalPlayerEnergy)
        {
            if (memoryBankNetIds.Count == 0)
            {
                return false;
            }

            MemoryBankComponent[] memoryBanks = new MemoryBankComponent[memoryBankNetIds.Count];

            for (int i = 0; i < memoryBankNetIds.Count; i++)
            {
                uint netId = memoryBankNetIds[i];

                if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity))
                {
                    MemoryBankComponent memoryBank = identity.GetComponent<MemoryBankComponent>();
                    memoryBanks[i] = memoryBank;
                }
            }

            bool canPlace = false;

            foreach (MemoryBankComponent memoryBank in memoryBanks)
            {
                if (memoryBank == null)
                {
                    continue;
                }

                int placementCost = memoryBank.PlaceableConfig.Cost;

                if (totalPlayerEnergy >= placementCost)
                {
                    canPlace = true;
                    break;
                }
            }

            return canPlace;
        }
    }
}
