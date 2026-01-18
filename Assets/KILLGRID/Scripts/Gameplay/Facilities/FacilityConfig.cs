using System;
using UnityEngine;

namespace KILLGRID.Gameplay.Facilities
{
    [Serializable]
    public class FacilityConfig
    {
        [SerializeField] private string name;
        [SerializeField] private FacilityType type;
        [SerializeField] private int cost;

        public string Name => name;
        public FacilityType Type => type;
        public int Cost => cost;
    }
}
