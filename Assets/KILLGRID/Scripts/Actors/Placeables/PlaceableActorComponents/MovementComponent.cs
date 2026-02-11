using System;
using System.Collections.Generic;
using Attic.DI;
using Attic.Utilities;
using Attic.Utils.Invoking;
using DG.Tweening;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Gameplay.HexGrid;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables.PlaceableActorComponents
{
    [Serializable]
    public class MovementConfig
    {
        [SerializeField] private int movementRange = 1;

        public int MovementRange => movementRange;
    }

    public class MovementComponent : PlaceableActorComponent
    {
        [Inject] private HexGridActor hexGridActor;
        [Inject] private InvokeWrapper invokeWrapper;

        [SerializeField] private MovementConfig config;

        public event Action<MovementComponent> MovementFinishedEvent;

        [Server]
        protected override void OnServerPlacedInternal()
        {

        }

        [Server]
        protected override void OnServerRemovedInternal()
        {

        }

        [Server]
        protected override void OnServerTurnStartInternal()
        {

        }

        [Server]
        protected override void OnServerTurnEndInternal()
        {

        }

        [Command(requiresAuthority = false)]
        private void Cmd_TryMove()
        {
            HexTileActor occupyingTile = Owner.OccupyingTile;
            TileCoords tileCoords = occupyingTile.GetTileCoords();

            HexTileActor[] oppositeEdgeTiles = hexGridActor.GetOppositeEdgeTiles(Owner.OwningPlayerIndex);

            List<(int q, int r)> shortestPath = null;
            int shortestPathLength = int.MaxValue;

            for (int i = 0; i < oppositeEdgeTiles.Length; i++)
            {
                HexTileActor oppositeEdgeTile = oppositeEdgeTiles[i];
                TileCoords oppositeTileCoords = oppositeEdgeTile.GetTileCoords();

                bool[,] blockedTiles = hexGridActor.GetBlockedTiles();

                List<(int q, int r)> path = HexGridPathfinder.FindPath((hexGridActor.GridData.GridWidth, hexGridActor.GridData.GridHeight),
                    tileCoords.ToTuple(), oppositeTileCoords.ToTuple(),
                    (q, r) => blockedTiles.GetValue(q, r) is true);

                if (path != null && path.Count < shortestPathLength)
                {
                    shortestPath = path;
                    shortestPathLength = path.Count;
                }
            }

            if (shortestPath == null || shortestPath.Count == 0)
            {
                return;
            }

            List<HexTileActor> pathTileActors = new List<HexTileActor>();

            foreach ((int x, int y) path in shortestPath)
            {
                HexTileActor hexTileActor = hexGridActor.GetGridTile(path.x, path.y);

                if (!hexTileActor)
                {
                    Log.Error($"Cannot find hex tile actor at path coords x:{path.x} y:{path.y}.");
                    return;
                }

                if (pathTileActors.Count > config.MovementRange)
                {
                    break;
                }

                pathTileActors.Add(hexTileActor);
            }

            occupyingTile.Cmd_RemoveActor();

            // Instead of animating on the server, send the path to all clients to animate locally
            List<Vector3> pathPositions = new List<Vector3>();
            foreach (HexTileActor tileActor in pathTileActors)
            {
                pathPositions.Add(tileActor.transform.position);
            }

            Rpc_AnimateMovement(pathPositions.ToArray());

            // Place the actor on the final tile on the server (logic only)
            invokeWrapper.Invoke(() => pathTileActors[^1].Cmd_PlaceActor(Owner), 0.75f * pathTileActors.Count);
        }

        [ClientRpc]
        private void Rpc_AnimateMovement(Vector3[] pathPositions)
        {
            // Animate the movement locally on each client
            Sequence sequence = DOTween.Sequence();

            foreach (Vector3 pos in pathPositions)
            {
                sequence.Append(transform.DOMove(pos, 0.75f));
            }

            sequence.OnComplete(() => {
                MovementFinishedEvent?.Invoke(this);
            });
        }

        [Client]
        public void RequestTryMove()
        {
            Cmd_TryMove();
        }
    }
}
