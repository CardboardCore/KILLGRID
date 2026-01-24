using UnityEngine;

namespace KILLGRID.Gameplay.MemoryBanks
{
    [CreateAssetMenu(fileName = "MemoryBankConfigs", menuName = "KILLGRID/MemoryBankConfigs")]
    public class MemoryBankConfigs : ScriptableObject
    {
        [SerializeField] private MemoryBankConfig[] memoryBankConfigs;

        public bool TryGetMemoryBankConfig(MemoryBankType memoryBankType, out MemoryBankConfig config)
        {
            foreach (MemoryBankConfig memoryBankConfig in memoryBankConfigs)
            {
                if (memoryBankConfig.MemoryBankType == memoryBankType)
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
