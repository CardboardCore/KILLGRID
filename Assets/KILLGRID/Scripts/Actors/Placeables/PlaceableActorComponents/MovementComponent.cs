using System;
using System.Collections.Generic;
using Attic.DI;
using Attic.Utilities;
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

        [SerializeField] private MovementConfig config;

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

                List<(int q, int r)> path = HexGridPathfinder.FindPath(tileCoords.ToTuple(), oppositeTileCoords.ToTuple(), null);

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

            Sequence sequence = DOTween.Sequence();

            foreach (HexTileActor pathTileActor in pathTileActors)
            {
                sequence.Append(transform.DOMove(pathTileActor.transform.position, 0.75f));
            }

            sequence.OnComplete(() => {
                pathTileActors[^1].Cmd_PlaceActor(Owner);
            });
        }

        [Client]
        public void RequestTryMove()
        {
            Cmd_TryMove();
        }
    }
}
