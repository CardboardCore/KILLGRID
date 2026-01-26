using Attic.DI;
using Attic.Mirror;
using Attic.Utilities;
using KILLGRID.Actors.TableButtons;
using KILLGRID.Gameplay.Placeables;
using Mirror;
using UnityEngine;

namespace KILLGRID.Gameplay.MemoryBanks
{
    [Injectable]
    public class MemoryBankFactory : AtticNetworkBehaviour
    {
        [SerializeField] private MemoryBankConfigs memoryBankConfigs;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        public MemoryBankComponent Spawn(PlaceableType placeableType)
        {
            if (!memoryBankConfigs.TryGetMemoryBankConfig(placeableType, out MemoryBankConfig config))
            {
                Log.Error($"No placeable config found for type {placeableType}");
                return null;
            }

            MemoryBankComponent memoryBank = config.MemoryBankPrefab;
            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            MemoryBankComponent memoryBankInstance = Instantiate(memoryBank, spawnPosition, spawnRotation);
            NetworkServer.Spawn(memoryBankInstance.gameObject);

            return memoryBankInstance;
        }
    }
}
