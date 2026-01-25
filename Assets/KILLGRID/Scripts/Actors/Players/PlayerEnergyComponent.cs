using System;
using System.Collections.Generic;
using Attic.Mirror.Actors.Components;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.Placeables.PlaceableActorComponents;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Players
{
    public class PlayerEnergyComponent : ActorComponent
    {
        [SerializeField] private PlayerOwnedTilesComponent playerOwnedTilesComponent;

        [SyncVar] private int totalEnergy = -1;

        public int TotalEnergy => totalEnergy;

        public event Action<int> EnergyUpdatedEvent;

        [Command]
        private void Cmd_UpdateEnergy()
        {
            int total = 0;

            IReadOnlyList<uint> ownedTileNetIds = playerOwnedTilesComponent.OwnedTileNetIds;

            foreach (uint id in ownedTileNetIds)
            {
                if (!NetworkServer.spawned.TryGetValue(id, out NetworkIdentity identity))
                {
                    continue;
                }

                HexTileActor hexTileActor = identity.GetComponent<HexTileActor>();

                if (!hexTileActor || !hexTileActor.TryGetPlacedActor(out PlaceableActor placeableActor))
                {
                    continue;
                }

                GeneratorComponent generatorComponent = placeableActor.GetComponent<GeneratorComponent>();

                if (!generatorComponent)
                {
                    continue;
                }

                int energyProduced = generatorComponent.GetEnergyProduction();
                total += energyProduced;
            }

            totalEnergy = total;

            Rpc_TotalEnergyUpdated(total);
        }

        [ClientRpc]
        private void Rpc_TotalEnergyUpdated(int newValue)
        {
            EnergyUpdatedEvent?.Invoke(newValue);
        }

        [Client]
        public void RequestUpdateEnergy()
        {
            Cmd_UpdateEnergy();
        }
    }
}
