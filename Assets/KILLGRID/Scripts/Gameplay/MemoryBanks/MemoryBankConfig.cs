using System;
using KILLGRID.Actors.TableButtons;
using UnityEngine;

namespace KILLGRID.Gameplay.MemoryBanks
{
    [Serializable]
    public class MemoryBankConfig
    {
        [SerializeField] private MemoryBankType memoryBankType;
        [SerializeField] private MemoryBankComponent memoryBankPrefab;

        public MemoryBankType MemoryBankType => memoryBankType;
        public MemoryBankComponent MemoryBankPrefab => memoryBankPrefab;
    }
}
