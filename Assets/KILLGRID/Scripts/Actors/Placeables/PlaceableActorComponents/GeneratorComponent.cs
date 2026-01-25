using System;
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
        [Serializable]
        public struct EnergizedTile : IEquatable<EnergizedTile>
        {
            public int x;
            public int y;

            public static EnergizedTile FromTuple((int x, int y) tuple)
            {
                return new EnergizedTile { x = tuple.x, y = tuple.y };
            }

            public bool Equals(EnergizedTile other)
            {
                return x == other.x && y == other.y;
            }

            public override bool Equals(object obj)
            {
                return obj is EnergizedTile other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y);
            }
        }

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

        private readonly SyncList<EnergizedTile> energizedTiles = new SyncList<EnergizedTile>();
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

                if (energizedTiles.Contains(EnergizedTile.FromTuple(nextTile)))
                {
                    continue;
                }

                if (!TryGetHexTileActorAtOffset(EnergizedTile.FromTuple(nextTile), out HexTileActor hexTile))
                {
                    continue;
                }

                Log.Write($"Energizing tile at offset x:{nextTile.x} y:{nextTile.y} for player index {Owner.OccupyingTile.OwnerPlayerIndex}.");

                hexTile.SetOwner(Owner.OccupyingTile.OwnerPlayerIndex);

                energizedTiles.Add(EnergizedTile.FromTuple(nextTile));
                lastDirectionIndex = nextIndex;

                break;
            }
        }

        [Server]
        private bool TryGetHexTileActorAtOffset(EnergizedTile offset, out HexTileActor hexTile)
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

        [Command(requiresAuthority = false)]
        private void Cmd_SpendEnergy(int amount)
        {
            // Get random energized tile to de-energize
            if (energizedTiles.Count == 0)
            {
                Log.Error($"Generator owned by player index {Owner.OccupyingTile.OwnerPlayerIndex} has no energized tiles to spend energy from.");
                return;
            }

            Log.Write($"De-energizing {amount} tiles for player index {Owner.OccupyingTile.OwnerPlayerIndex}.");

            if (amount > energizedTiles.Count)
            {
                Log.Exception($"Something went wrong trying to spend {amount} energy from generator owned by player index {Owner.OccupyingTile.OwnerPlayerIndex} which only has {energizedTiles.Count} energized tiles.");
            }

            int deEnergizeIndex = energizedTiles.Count - 1;

            // Find tiles counter-clockwise from the last energized tile
            for (int i = 0; i < amount; i++)
            {
                EnergizedTile tileToDeEnergize = energizedTiles[deEnergizeIndex];

                if (!TryGetHexTileActorAtOffset(tileToDeEnergize, out HexTileActor hexTile))
                {
                    Log.Error($"Failed to find hex tile at offset x:{tileToDeEnergize.x} y:{tileToDeEnergize.y} to de-energize.");
                    return;
                }

                Log.Write($"De-energizing tile at offset x:{tileToDeEnergize.x} y:{tileToDeEnergize.y} for player index {Owner.OccupyingTile.OwnerPlayerIndex}.");

                // De-energize the tile
                hexTile.SetOwner(-1);

                // Error here!!
                energizedTiles.RemoveAt(deEnergizeIndex);

                deEnergizeIndex -= 1;
            }

            lastDirectionIndex = deEnergizeIndex;
        }

        [Server]
        protected override void OnServerPlacedInternal()
        {
            // energizedTiles.Add(Owner.OccupyingTile.GetGridPosition());
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

        [Server]
        protected override void OnServerTurnEndInternal()
        {
            // No action needed on turn end for the generator
        }

        public int GetEnergyProduction()
        {
            return energizedTiles.Count;
        }

        [Client]
        public void RequestSpendEnergy(int amount)
        {
            Cmd_SpendEnergy(amount);
        }
    }
}
