using System;
using KILLGRID.Gameplay.BoardPlacement;
using UnityEngine;

namespace KILLGRID.Gameplay.Facilities
{
    [Serializable]
    public class FacilityConfig
    {
        [SerializeField] private string name;
        [SerializeField] private FacilityType type;
        [SerializeField] private int cost;
        [SerializeField] private BoardPlaceableConfig placeableConfig;

        public string Name => name;
        public FacilityType Type => type;
        public int Cost => cost;
        public BoardPlaceableConfig PlaceableConfig => placeableConfig;
    }
}
