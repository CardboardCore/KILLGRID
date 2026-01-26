using System;
using KILLGRID.Actors.TableButtons;
using KILLGRID.Gameplay.Placeables;
using UnityEngine;

namespace KILLGRID.Gameplay.MemoryBanks
{
    [Serializable]
    public class MemoryBankConfig
    {
        [SerializeField] private PlaceableType placeableType;
        [SerializeField] private MemoryBankComponent memoryBankPrefab;

        public PlaceableType PlaceableType => placeableType;
        public MemoryBankComponent MemoryBankPrefab => memoryBankPrefab;
    }
}
