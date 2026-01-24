using System.Collections.Generic;
using Attic.DI;
using Attic.Mirror.Actors;
using Attic.Utilities;
using Attic.Utils.Invoking;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.Placeables.PlaceableActorComponents;
using KILLGRID.Actors.TableButtons;
using KILLGRID.Gameplay.Placeables;
using UnityEngine;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class InsertMemoryBankState : PlayerActionState
    {
        [Inject] private PlaceablesFactory placeablesFactory;
        [Inject] private ActorCleaner actorCleaner;
        [Inject] private InvokeWrapper invokeWrapper;

        private MemoryBankComponent memoryBankComponent;
        private bool isSpawning;
        private PlaceableActor placeableActor;

        protected override void OnEnter()
        {
            base.OnEnter();

            PlayerInteractComponent playerInteractComponent = owningStateMachine.Owner.GetComponent<PlayerInteractComponent>();
            memoryBankComponent = playerInteractComponent.CurrentHighlightedInteractableComponent.GetComponent<MemoryBankComponent>();

            isSpawning = false;
            placeableActor = null;
        }

        protected override void OnHover(InteractableComponent interactableComponent)
        {
            if (!memoryBankComponent || !interactableComponent)
            {
                return;
            }

            if (interactableComponent.InteractableConfig.InteractableType != InteractableType.Tile)
            {
                return;
            }

            Log.Write($"Hovering over tile to insert placeable from memory bank: {memoryBankComponent.PlaceableConfig.Name}");

            // Show placeable as hologram on tile
            HexTileActor hexTileActor = interactableComponent.Owner as HexTileActor;

            if (!hexTileActor)
            {
                return;
            }

            if (placeableActor)
            {
                hexTileActor.MoveActorToTile(placeableActor);

                if (hexTileActor.IsOccupied)
                {
                    // Show red hologram
                    placeableActor.ShowAsRedHologram(true);
                }
                else
                {
                    placeableActor.ShowAsGreenHologram(true);
                }
            }
            else if (!isSpawning)
            {
                Log.Write("Spawning placeable hologram...");

                isSpawning = true;

                placeablesFactory.PlaceableSpawnedEvent += OnPlaceableSpawned;
                placeablesFactory.RequestSpawn(owningStateMachine.Owner, memoryBankComponent.PlaceableConfig.Type, hexTileActor);

                void OnPlaceableSpawned(PlaceableActor placeable)
                {
                    Log.Write("Placeable hologram spawned.");

                    placeablesFactory.PlaceableSpawnedEvent -= OnPlaceableSpawned;

                    placeable.WhenSpawned(() => {

                        Log.Write("Configuring placeable hologram...");

                        placeable.RequestTakeOwnership(owningStateMachine.Owner);

                        placeable.ShowAsGreenHologram(true);
                        placeable.transform.position = hexTileActor.transform.position;

                        placeableActor = placeable;
                        isSpawning = false;
                    });
                }
            }
        }

        protected override void OnSelect(InteractableComponent interactableComponent)
        {
            switch (interactableComponent.InteractableConfig.InteractableType)
            {
                case InteractableType.MemoryBank:

                    // Cancel insertion
                    if (placeableActor)
                    {
                        actorCleaner.RequestQueueForRemoval(placeableActor);
                        placeableActor = null;
                        memoryBankComponent = null;
                    }

                    ToState<AwaitingPlayerSelectionState>();

                    break;

                case InteractableType.Tile:

                    // Place placeable on tile and spend memory bank / energy
                    if (placeableActor && memoryBankComponent)
                    {
                        HexTileActor hexTileActor = interactableComponent.Owner as HexTileActor;

                        if (!hexTileActor)
                        {
                            Log.Error($"Actor {interactableComponent.Owner.name} is not a HexTileActor, but has InteractableType.Tile");
                            return;
                        }

                        PlayerOwnedTilesComponent playerOwnedTilesComponent = owningStateMachine.Owner.GetComponent<PlayerOwnedTilesComponent>();

                        // Get all generators and spend energy based on memory bank cost
                        GeneratorComponent[] allOwnedGenerators = playerOwnedTilesComponent.GetAllOwnedTilesWithComponent<GeneratorComponent>();

                        int remainingToSpend = memoryBankComponent.PlaceableConfig.Cost;

                        // TODO: Use a better algorithm to spend energy from generators
                        Dictionary<GeneratorComponent, int> energySpentPerGenerator = new Dictionary<GeneratorComponent, int>();

                        foreach (GeneratorComponent generator in allOwnedGenerators)
                        {
                            if (remainingToSpend <= 0)
                            {
                                break;
                            }

                            int energyAvailable = generator.GetEnergyProduction();

                            if (energyAvailable <= 0)
                            {
                                continue;
                            }

                            int energyToSpend = Mathf.Min(energyAvailable, remainingToSpend);

                            energySpentPerGenerator.Add(generator, energyToSpend);

                            Log.Write($"Mapping to spend {energyToSpend} energy from generator on tile {generator.name}");

                            remainingToSpend -= energyToSpend;
                        }

                        if (remainingToSpend > 0)
                        {
                            Log.Exception($"Not enough energy to spend for placing memory bank. Still need to spend {remainingToSpend} energy.");
                        }

                        foreach (KeyValuePair<GeneratorComponent, int> keyValuePair in energySpentPerGenerator)
                        {
                            keyValuePair.Key.RequestSpendEnergy(keyValuePair.Value);
                        }

                        memoryBankComponent.RequestConsume();

                        playerOwnedTilesComponent.AddOwnedTile(hexTileActor);

                        owningStateMachine.Owner.GetComponent<PlayerMemoryBankComponent>().Cmd_RemoveMemoryBank(memoryBankComponent.netId);

                        hexTileActor.RequestPlaceActor(placeableActor);

                        placeableActor.ShowAsNormal(true);


                        placeableActor = null;
                        memoryBankComponent = null;

                        // TODO: Somehow make sure all data is synced before going to next state
                        invokeWrapper.Invoke(ToState<CheckEnergyState>, 1f);
                    }

                    break;
            }
        }
    }
}
