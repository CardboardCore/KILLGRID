using System.Linq;
using UnityEngine;

namespace KILLGRID.Gameplay.Facilities
{
    [CreateAssetMenu(fileName = "FacilitiesConfig", menuName = "KILLGRID/FacilitiesConfig")]
    public class FacilitiesConfig : ScriptableObject
    {
        [SerializeField] private FacilityConfig[] facilities;

        /// <summary>
        /// Gets facility data by type. Only one facility per type is supported.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="facilityConfig"></param>
        /// <returns></returns>
        public bool TryGetFacilityData(FacilityType type, out FacilityConfig facilityConfig)
        {
            facilityConfig = facilities.FirstOrDefault(facility => facility.Type == type);
            return facilityConfig != null;
        }
    }
}
