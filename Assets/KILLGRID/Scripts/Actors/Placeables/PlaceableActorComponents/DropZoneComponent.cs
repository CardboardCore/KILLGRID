using Attic.DI;
using KILLGRID.Actors.HexGrid;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables.PlaceableActorComponents
{
    public class DropZoneComponent : PlaceableActorComponent
    {
        [Inject] private HexGridActor hexGridActor;

        [SerializeField] private int range = 1;

        private readonly SyncList<TileCoords> droppableTiles = new SyncList<TileCoords>();

        [Command(requiresAuthority = false)]
        private void Cmd_ShowDropZone()
        {
            foreach (TileCoords tileCoords in droppableTiles)
            {
                if (!TryGetHexTileActorAtOffset(tileCoords, out HexTileActor hexTile))
                {
                    continue;
                }

                hexTile.ShowBuildZone(true);
            }
        }

        [Command(requiresAuthority = false)]
        public void Cmd_HideDropZone()
        {
            foreach (TileCoords tileCoords in droppableTiles)
            {
                if (!TryGetHexTileActorAtOffset(tileCoords, out HexTileActor hexTile))
                {
                    continue;
                }

                hexTile.ShowBuildZone(false);
            }
        }

        [Server]
        private bool TryGetHexTileActorAtOffset(TileCoords offset, out HexTileActor hexTile)
        {
            // Assuming Owner has a method to get its current tile position
            (int ownerX, int ownerY) = Owner.OccupyingTile.GetGridPosition();
            (int targetX, int targetY) = (ownerX + offset.x, ownerY + offset.y);

            hexTile = hexGridActor.GetGridTile(targetX, targetY);

            return hexTile != null;
        }

        [Server]
        protected override void OnServerPlacedInternal()
        {
            (int ownerX, int ownerY) = Owner.OccupyingTile.GetGridPosition();

            droppableTiles.Add(TileCoords.FromTuple((0, 0)));

            for (int r = 1; r <= range; r++)
            {
                int cx = 0;
                int cy = 0;

                // Start at the "SW" corner (offsets[4])
                for (int i = 0; i < r; i++)
                {
                    int parity = (ownerX + cx) % 2;
                    (int x, int y)[] offsets = parity == 0 ? HexGridUtils.EvenQOffsets : HexGridUtils.OddQOffsets;
                    cx += offsets[4].x;
                    cy += offsets[4].y;
                }

                // Walk the ring
                for (int side = 0; side < 6; side++)
                {
                    for (int step = 0; step < r; step++)
                    {
                        TileCoords offset = TileCoords.FromTuple((cx, cy));
                        if (TryGetHexTileActorAtOffset(offset, out HexTileActor hexTile))
                        {
                            if (!droppableTiles.Contains(offset))
                            {
                                droppableTiles.Add(offset);
                            }
                        }

                        int parity = (ownerX + cx) % 2;
                        (int x, int y)[] offsets = parity == 0 ? HexGridUtils.EvenQOffsets : HexGridUtils.OddQOffsets;
                        cx += offsets[side].x;
                        cy += offsets[side].y;
                    }
                }
            }
        }

        [Server]
        protected override void OnServerTurnStartInternal()
        {

        }

        [Server]
        protected override void OnServerTurnEndInternal()
        {

        }

        [Client]
        public void RequestShowDropZone()
        {
            Cmd_ShowDropZone();
        }

        [Client]
        public void RequestHideDropZone()
        {
            Cmd_HideDropZone();
        }
    }
}
