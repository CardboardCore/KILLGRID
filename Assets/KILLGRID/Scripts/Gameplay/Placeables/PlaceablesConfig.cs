using UnityEngine;

namespace KILLGRID.Gameplay.Placeables
{
    [CreateAssetMenu(fileName = "PlaceablesConfig", menuName = "KILLGRID/PlaceablesConfig")]
    public class PlaceablesConfig : ScriptableObject
    {
        [SerializeField] private PlaceableConfig[] placeables;

        public bool TryGetPlaceableConfig(PlaceableType type, out PlaceableConfig config)
        {
            foreach (var placeable in placeables)
            {
                if (placeable.Type == type)
                {
                    config = placeable;
                    return true;
                }
            }

            config = null;
            return false;
        }
    }
}
