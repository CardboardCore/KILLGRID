using Attic.Mirror.Actors.Components;
using UnityEngine;

namespace KILLGRID.Actors.Players
{
    public class PlayerEnergyComponent : ActorComponent
    {
        [SerializeField] private PlayerOwnedTilesComponent playerOwnedTilesComponent;

        // Get all placeable actors placed on owned tiles
        // Find the core hq and generator facilities
        // Check their energy production
    }
}
