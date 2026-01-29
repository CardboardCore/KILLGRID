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
using KILLGRID.Input;
using UnityEngine;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class InsertMemoryBankState : PlayerActionState
    {
        [Inject] private InputManager inputManager;
        [Inject] private PlaceablesFactory placeablesFactory;
        [Inject] private ActorCleaner actorCleaner;
        [Inject] private InvokeWrapper invokeWrapper;

        private MemoryBankComponent memoryBankComponent;
        private bool isSpawning;
        private PlaceableActor placeableActor;

        private PlayerOwnedTilesComponent playerOwnedTilesComponent;

        private bool canPlace;

        protected override void OnEnter()
        {
            base.OnEnter();

            inputManager.Player.Enable();

            PlayerInteractComponent playerInteractComponent = owningStateMachine.Owner.GetComponent<PlayerInteractComponent>();
            memoryBankComponent = playerInteractComponent.CurrentHighlightedInteractableComponent.GetComponent<MemoryBankComponent>();

            isSpawning = false;
            placeableActor = null;

            playerOwnedTilesComponent = owningStateMachine.Owner.GetComponent<PlayerOwnedTilesComponent>();

            if (memoryBankComponent.PlaceableConfig.Type.IsBuilding())
            {
                BuildZoneComponent[] buildZoneComponents = playerOwnedTilesComponent.GetAllOwnedTilesWithPlaceableComponent<BuildZoneComponent>();

                foreach (BuildZoneComponent buildZoneComponent in buildZoneComponents)
                {
                    buildZoneComponent.RequestShowBuildZone();
                }
            }

            if (memoryBankComponent.PlaceableConfig.Type.IsUnit())
            {
                DropZoneComponent[] dropZoneComponents = playerOwnedTilesComponent.GetAllOwnedTilesWithPlaceableComponent<DropZoneComponent>();

                foreach (DropZoneComponent dropZoneComponent in dropZoneComponents)
                {
                    dropZoneComponent.RequestShowDropZone();
                }
            }

            // TODO: Based on memory bank type, show different highlights on valid tiles (via either DropZoneComponent or BuildZoneComponent)

            // Get all build zone components for this player and highlight valid tiles
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
                UpdatePlaceableActor(placeableActor);
            }
            else if (!isSpawning)
            {
                Log.Write("Spawning placeable hologram...");

                isSpawning = true;

                placeablesFactory.PlaceableSpawnedEvent += OnPlaceableSpawned;
                placeablesFactory.RequestSpawn(owningStateMachine.Owner, memoryBankComponent.PlaceableConfig.Type, hexTileActor);
            }

            return;

            void OnPlaceableSpawned(PlaceableActor placeable)
            {
                Log.Write("Placeable hologram spawned.");

                placeablesFactory.PlaceableSpawnedEvent -= OnPlaceableSpawned;

                placeable.WhenSpawned(() => {

                    Log.Write("Configuring placeable hologram...");

                    placeable.RequestTakeOwnership(owningStateMachine.Owner);

                    placeable.ShowAsGreenHologram(true);
                    placeable.transform.position = hexTileActor.transform.position;

                    UpdatePlaceableActor(placeable);

                    placeableActor = placeable;
                    isSpawning = false;
                });
            }

            void UpdatePlaceableActor(PlaceableActor placeable)
            {
                hexTileActor.MoveActorToTile(placeable);

                if (memoryBankComponent.PlaceableConfig.Type == PlaceableType.CoreHQBuilding || hexTileActor.IsEligibleForPlacement)
                {
                    placeable.ShowAsGreenHologram(true);
                    canPlace = true;
                }
                else
                {
                    placeable.ShowAsRedHologram(true);
                    canPlace = false;
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

                        // Get all generators and spend energy based on memory bank cost
                        GeneratorComponent[] allOwnedGenerators = playerOwnedTilesComponent.GetAllOwnedTilesWithPlaceableComponent<GeneratorComponent>();

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

                        if (memoryBankComponent.PlaceableConfig.Type.IsBuilding())
                        {
                            BuildZoneComponent[] buildZoneComponents = playerOwnedTilesComponent.GetAllOwnedTilesWithPlaceableComponent<BuildZoneComponent>();

                            foreach (BuildZoneComponent buildZoneComponent in buildZoneComponents)
                            {
                                buildZoneComponent.RequestHideBuildZone();
                            }
                        }

                        if (memoryBankComponent.PlaceableConfig.Type.IsUnit())
                        {
                            DropZoneComponent[] dropZoneComponents = playerOwnedTilesComponent.GetAllOwnedTilesWithPlaceableComponent<DropZoneComponent>();

                            foreach (DropZoneComponent dropZoneComponent in dropZoneComponents)
                            {
                                dropZoneComponent.RequestHideDropZone();
                            }
                        }

                        placeableActor = null;
                        memoryBankComponent = null;
                        playerOwnedTilesComponent = null;

                        inputManager.Player.Disable();

                        // TODO: Somehow make sure all data is synced before going to next state
                        invokeWrapper.Invoke(ToState<CheckEnergyState>, 1f);
                    }

                    break;
            }
        }
    }
}
