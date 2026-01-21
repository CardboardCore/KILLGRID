using System;
using KILLGRID.Actors.Placeables;
using UnityEngine;

namespace KILLGRID.Gameplay.Placeables
{
    [Serializable]
    public class PlaceableConfig
    {
        [SerializeField] private string name;
        [SerializeField] private PlaceableType type;
        [SerializeField] private int cost;
        [SerializeField] private PlaceableActor placeableActorPrefab;

        public string Name => name;
        public PlaceableType Type => type;
        public int Cost => cost;
        public PlaceableActor PlaceableActorPrefab => placeableActorPrefab;
    }
}
