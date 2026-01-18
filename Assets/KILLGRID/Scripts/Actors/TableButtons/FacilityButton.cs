using System;
using Attic.Mirror.Actors.Components;
using Attic.Utilities;
using KILLGRID.Gameplay.Facilities;
using TMPro;
using UnityEngine;

namespace KILLGRID.Actors.TableButtons
{
    [Serializable]
    public class FacilityButtonConfig
    {
        [SerializeField] private FacilityType facilityType;

        public FacilityType FacilityType => facilityType;
    }

    public class FacilityButton : ActorComponent
    {
        [Header("References")]
        [SerializeField] private FacilitiesConfig facilitiesConfig;
        [SerializeField] private TextMeshPro nameText;
        [SerializeField] private TextMeshPro costText;

        [Header("Settings")]
        [SerializeField] private FacilityButtonConfig facilityButtonConfig;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!facilitiesConfig.TryGetFacilityData(facilityButtonConfig.FacilityType, out FacilityConfig config))
            {
                Log.Error($"No facility config found for type {facilityButtonConfig.FacilityType}");
                return;
            }

            nameText.text = config.Name;
            costText.text = config.Cost.ToString();
        }
    }
}
