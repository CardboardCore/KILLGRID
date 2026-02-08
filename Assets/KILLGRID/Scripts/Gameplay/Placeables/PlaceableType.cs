using UnityEngine;

namespace KILLGRID.Gameplay.Placeables
{
    public enum PlaceableType
    {
        // Buildings
        CoreHQBuilding = 0,
        GeneratorBuilding = 1,
        UplinkBuilding = 2,
        SubstationBuilding = 3,

        // Units
        RunnerUnit = 10,
        EnforcerUnit = 11,
        PhantomUnit = 12
    }

    public static class PlaceableTypeExtensions
    {
        public static readonly PlaceableType[] NonCoreBuildingTypes = new[]
        {
            PlaceableType.GeneratorBuilding, PlaceableType.UplinkBuilding, PlaceableType.SubstationBuilding
        };

        public static readonly PlaceableType[] UnitTypes = new[]
        {
            PlaceableType.RunnerUnit, PlaceableType.EnforcerUnit, PlaceableType.PhantomUnit
        };

        public static readonly PlaceableType[] BuildingTypes = new[]
        {
            PlaceableType.CoreHQBuilding, PlaceableType.GeneratorBuilding, PlaceableType.UplinkBuilding, PlaceableType.SubstationBuilding
        };

        public static bool IsBuilding(this PlaceableType placeableType)
        {
            return placeableType is PlaceableType.CoreHQBuilding or PlaceableType.GeneratorBuilding or PlaceableType.UplinkBuilding or PlaceableType.SubstationBuilding;
        }

        public static bool IsUnit(this PlaceableType placeableType)
        {
            return placeableType is PlaceableType.RunnerUnit or PlaceableType.EnforcerUnit or PlaceableType.PhantomUnit;
        }

        /// <summary>
        /// Includes all unit types.
        /// </summary>
        /// <returns></returns>
        public static PlaceableType GetRandomUnitType()
        {
            int index = UnityEngine.Random.Range(0, UnitTypes.Length);

            return UnitTypes[index];
        }

        /// <summary>
        /// Includes all building types.
        /// </summary>
        /// <returns></returns>
        public static PlaceableType GetRandomBuildingType()
        {
            int index = UnityEngine.Random.Range(0, BuildingTypes.Length);

            return BuildingTypes[index];
        }

        /// <summary>
        /// Includes all unit types and non-core building types.
        /// </summary>
        /// <returns></returns>
        public static PlaceableType GetRandomNonCoreType()
        {
            float random = Random.Range(0f, 1f);

            PlaceableType randomType = random < 0.5f
                ? GetRandomUnitType()
                : GetRandomNonCoreBuildingType();

            return randomType;
        }

        /// <summary>
        /// Includes only non-core building types.
        /// </summary>
        /// <returns></returns>
        public static PlaceableType GetRandomNonCoreBuildingType()
        {
            int index = UnityEngine.Random.Range(0, NonCoreBuildingTypes.Length);

            return NonCoreBuildingTypes[index];
        }
    }
}
