using System.Collections.Generic;
using Attic.DI;
using Attic.Utilities;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Input;
using Mirror;
using UnityEngine.InputSystem;

namespace KILLGRID.Actors.Placeables.PlaceableActorComponents
{
    public class GeneratorComponent : PlaceableActorComponent
    {
        [Inject] private HexGridActor hexGridActor;
        [Inject] private InputManager inputManager;

        // Even-q offset for flat-top hex grid
        private static readonly (int x, int y)[] EvenQOffsets = new (int, int)[]
        {
            (0, 1),   // N
            (1, 0),   // NE
            (1, -1),  // SE
            (0, -1),  // S
            (-1, -1), // SW
            (-1, 0)   // NW
        };

        // Odd-q offset for flat-top hex grid
        private static readonly (int x, int y)[] OddQOffsets = new (int, int)[]
        {
            (0, 1),  // N
            (1, 1),  // NE
            (1, 0),  // SE
            (0, -1), // S
            (-1, 0), // SW
            (-1, 1)  // NW
        };

        private List<(int x, int y)> energizedTiles = new List<(int, int)>();
        private int lastDirectionIndex = 5;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isServer)
            {
                return;
            }

            inputManager.Cheats.IncrementGenerator.performed += OnCheatIncrementGeneratorPerformed;
        }

        protected override void OnReleased()
        {
            if (isServer)
            {
                inputManager.Cheats.IncrementGenerator.performed -= OnCheatIncrementGeneratorPerformed;
            }

            base.OnReleased();
        }

        private void OnCheatIncrementGeneratorPerformed(InputAction.CallbackContext obj)
        {
            AddNextClockwiseTile();
        }

        [Server]
        private void AddNextClockwiseTile()
        {
            if (Owner.OccupyingTile.OwnerPlayerIndex == -1)
            {
                (int x, int y) = Owner.OccupyingTile.GetGridPosition();
                Log.Error($"Tile x:{x} y:{y} has no owner player index.");

                return;
            }

            int myX = Owner.OccupyingTile.GetGridPosition().x;
            (int x, int y)[] offsets = (myX % 2 == 0) ? EvenQOffsets : OddQOffsets;

            for (int i = 1; i <= offsets.Length; i++)
            {
                int nextIndex = (lastDirectionIndex + i) % offsets.Length;
                (int x, int y) nextTile = offsets[nextIndex];

                if (energizedTiles.Contains(nextTile))
                {
                    continue;
                }

                if (!TryGetHexTileAtOffset(nextTile, out HexTileActor hexTile))
                {
                    continue;
                }

                Log.Write($"Energizing tile at offset x:{nextTile.x} y:{nextTile.y} for player index {Owner.OccupyingTile.OwnerPlayerIndex}.");

                hexTile.SetOwner(Owner.OccupyingTile.OwnerPlayerIndex);

                energizedTiles.Add(nextTile);
                lastDirectionIndex = nextIndex;

                break;
            }
        }

        [Server]
        private bool TryGetHexTileAtOffset((int x, int y) offset, out HexTileActor hexTile)
        {
            // Assuming Owner has a method to get its current tile position
            (int ownerX, int ownerY) = Owner.OccupyingTile.GetGridPosition();
            (int targetX, int targetY) = (ownerX + offset.x, ownerY + offset.y);

            hexTile = hexGridActor.GetGridTile(targetX, targetY);

            if (hexTile == null)
            {
                return false;
            }

            return true;
        }

        [Server]
        protected override void OnServerPlacedInternal()
        {
            energizedTiles.Add(Owner.OccupyingTile.GetGridPosition());
        }

        [Server]
        protected override void OnServerTurnStartInternal()
        {
            int myX = Owner.OccupyingTile.GetGridPosition().x;
            (int x, int y)[] offsets = (myX % 2 == 0) ? EvenQOffsets : OddQOffsets;

            if (energizedTiles.Count < offsets.Length)
            {
                AddNextClockwiseTile();
            }
        }
    }
}
