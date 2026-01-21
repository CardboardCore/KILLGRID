using System;
using System.Collections.Generic;
using KILLGRID.Actors.HexGrid;
using Mirror;

namespace KILLGRID.Actors.Placeables.PlaceableActorComponents
{
    public class GeneratorComponent : PlaceableActorComponent
    {
        // Clockwise neighbor offsets for hex grid (relative to center)
        private static readonly (int x, int y)[] NeighborOffsets = new (int, int)[]
        {
            (1, 0),  // East
            (1, -1), // South-East
            (0, -1), // South-West
            (-1, 0), // West
            (-1, 1), // North-West
            (0, 1)   // North-East
        };

        private List<(int x, int y)> energizedTiles = new List<(int, int)>();
        private int lastDirectionIndex = -1;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isServer)
            {
                return;
            }

            if (energizedTiles.Count == 0)
            {
                // Start with a random neighbor
                Random random = new Random();
                int startIndex = random.Next(NeighborOffsets.Length);
                energizedTiles.Add(NeighborOffsets[startIndex]);

                lastDirectionIndex = startIndex;
            }
        }

        private void AddNextClockwiseTile()
        {
            for (int i = 1; i <= NeighborOffsets.Length; i++)
            {
                int nextIndex = (lastDirectionIndex + i) % NeighborOffsets.Length;
                (int x, int y) nextTile = NeighborOffsets[nextIndex];

                if (energizedTiles.Contains(nextTile))
                {
                    continue;
                }

                energizedTiles.Add(nextTile);
                lastDirectionIndex = nextIndex;
                break;
            }
        }

        private bool TryGetHexTileAtOffset((int x, int y) offset, out HexTileActor hexTile)
        {
            // Assuming Owner has a method to get its current tile position
            (int ownerX, int ownerY) = Owner.OccupyingTile.GetGridPosition();
            (int targetX, int targetY) = (ownerX + offset.x, ownerY + offset.y);

            return HexGridManager.Instance.TryGetHexTileAt(targetX, targetY, out hexTile);
        }

        [Server]
        protected override void OnServerTurnStart()
        {
            if (energizedTiles.Count < NeighborOffsets.Length)
            {
                AddNextClockwiseTile();
            }
        }
    }
}
