using Attic.DI;
using Attic.Mirror.Actors;
using Attic.Utilities;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Interactables;
using KILLGRID.Actors.Placeables;
using KILLGRID.Actors.TableButtons;
using KILLGRID.Gameplay.Placeables;

namespace KILLGRID.Actors.Players.PlayerActions.States
{
    public class InsertMemoryBankState : PlayerActionState
    {
        [Inject] private PlaceablesFactory placeablesFactory;
        [Inject] private ActorCleaner actorCleaner;

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
            if (!interactableComponent)
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

                    ToState<IdleState>();

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

                        hexTileActor.RequestPlaceActor(placeableActor);

                        placeableActor.ShowAsNormal(true);

                        // memoryBankComponent.Consume();

                        placeableActor = null;
                        memoryBankComponent = null;

                        ToState<IdleState>();
                    }

                    break;
            }
        }
    }
}
