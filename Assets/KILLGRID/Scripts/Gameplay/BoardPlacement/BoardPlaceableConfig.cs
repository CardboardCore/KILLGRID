using System;
using KILLGRID.Actors.Placeables;
using UnityEngine;

namespace KILLGRID.Gameplay.BoardPlacement
{
    [Serializable]
    public class BoardPlaceableConfig
    {
        [SerializeField] private BoardPlaceableActor boardPlaceableActorPrefab;

        public BoardPlaceableActor BoardPlaceableActorPrefab => boardPlaceableActorPrefab;
    }
}
