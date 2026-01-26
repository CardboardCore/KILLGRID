using KILLGRID.Gameplay.Placeables;
using UnityEngine;

namespace KILLGRID.Gameplay.MemoryBanks
{
    [CreateAssetMenu(fileName = "MemoryBankConfigs", menuName = "KILLGRID/MemoryBankConfigs")]
    public class MemoryBankConfigs : ScriptableObject
    {
        [SerializeField] private MemoryBankConfig[] memoryBankConfigs;

        public bool TryGetMemoryBankConfig(PlaceableType placeableType, out MemoryBankConfig config)
        {
            foreach (MemoryBankConfig memoryBankConfig in memoryBankConfigs)
            {
                if (memoryBankConfig.PlaceableType == placeableType)
                {
                    config = memoryBankConfig;
                    return true;
                }
            }

            config = null;
            return false;
        }
    }
}
