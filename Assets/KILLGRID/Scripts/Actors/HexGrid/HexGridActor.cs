using System;
using Attic.DI;
using Attic.Mirror;
using KILLGRID.Actors.Placeables.PlaceableActorComponents;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.HexGrid
{
    [Serializable]
    public class HexGridData
    {
        [SerializeField] private HexTileActor tilePrefab;
        [SerializeField] private int gridWidth;
        [SerializeField] private int gridHeight;
        [SerializeField] private float spacing;
        [SerializeField] private float gridHeightOffset;

        public HexTileActor TilePrefab => tilePrefab;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float Spacing => spacing;
        public float GridHeightOffset => gridHeightOffset;
    }

    [Injectable]
    public class HexGridActor : AtticNetworkBehaviour
    {
        [SerializeField] private HexGridData gridData;

        private HexTileActor[,] hexTiles;
        private HexTileActor[] flatHexTiles;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        public void SpawnGrid()
        {
            hexTiles = new HexTileActor[gridData.GridWidth, gridData.GridHeight];
            flatHexTiles = new HexTileActor[gridData.GridWidth * gridData.GridHeight];

            for (int x = 0; x < gridData.GridWidth; x++)
            {
                for (int y = 0; y < gridData.GridHeight; y++)
                {
                    Vector3 tileSize = gridData.TilePrefab.GetComponentInChildren<Renderer>().bounds.size;

                    // Calculate position with offset for hexagonal layout
                    float xOffset = x * (tileSize.x * 0.75f + gridData.Spacing);
                    float yOffset = y * (tileSize.z + gridData.Spacing) + (x % 2 == 0 ? 0 : (tileSize.z + gridData.Spacing) / 2);

                    // Center the grid
                    xOffset -= (gridData.GridWidth - 1) * (tileSize.x * 0.75f + gridData.Spacing) / 2f;
                    yOffset -= (gridData.GridHeight - 1) * (tileSize.z + gridData.Spacing) / 2f;

                    // Final tweak to make it look better
                    yOffset += -0.05f;

                    Vector2 position = new Vector2(xOffset, yOffset);

                    HexTileActor hexTileActor = Instantiate(gridData.TilePrefab, new Vector3(position.x, gridData.GridHeightOffset, position.y), Quaternion.identity);
                    NetworkServer.Spawn(hexTileActor.gameObject);

                    hexTileActor.SetCoordinates(x, y);

                    hexTiles[x, y] = hexTileActor;
                    flatHexTiles[y * gridData.GridWidth + x] = hexTileActor;
                }
            }
        }

        [Server]
        public HexTileActor GetGridTile(int x, int y)
        {
            if (x < 0 || x >= gridData.GridWidth || y < 0 || y >= gridData.GridHeight)
            {
                return null;
            }

            return hexTiles[x, y];
        }

        [Server]
        public HexTileActor GetGridTile(TileCoords coords)
        {
            if (coords.x < 0 || coords.x >= gridData.GridWidth || coords.y < 0 || coords.y >= gridData.GridHeight)
            {
                return null;
            }

            return hexTiles[coords.x, coords.y];
        }

        [Server]
        public HexTileActor[] GetOppositeEdgeTiles(int playerIndex)
        {
            int edgeIndex = playerIndex == 0 ? gridData.GridHeight - 1 : 0;

            HexTileActor[] edgeTiles = new HexTileActor[gridData.GridWidth];

            for (int x = 0; x < gridData.GridWidth; x++)
            {
                edgeTiles[x] = hexTiles[x, edgeIndex];
            }

            return edgeTiles;
        }
    }
}
