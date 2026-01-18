using System;
using Attic.DI;
using Attic.Mirror.Actors.Components;
using Attic.Utilities;
using KILLGRID.Actors.Interactables;
using KILLGRID.Gameplay.BoardPlacement;
using KILLGRID.Gameplay.Facilities;
using Mirror;
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
        [Inject] private BoardPlaceableManager boardPlaceableManager;

        [Header("References")]
        [SerializeField] private FacilitiesConfig facilitiesConfig;
        [SerializeField] private InteractableComponent interactableComponent;
        [SerializeField] private TextMeshPro nameText;
        [SerializeField] private TextMeshPro costText;

        [Header("Settings")]
        [SerializeField] private FacilityButtonConfig facilityButtonConfig;

        private FacilityConfig myFacilityConfig;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!facilitiesConfig.TryGetFacilityData(facilityButtonConfig.FacilityType, out myFacilityConfig))
            {
                Log.Error($"No facility config found for type {facilityButtonConfig.FacilityType}");
                return;
            }

            nameText.text = myFacilityConfig.Name;
            costText.text = myFacilityConfig.Cost.ToString();

            interactableComponent.SelectEvent += OnSelect;
            interactableComponent.UnSelectEvent += OnUnselect;
        }

        protected override void OnReleased()
        {
            interactableComponent.SelectEvent -= OnSelect;
            interactableComponent.UnSelectEvent -= OnUnselect;

            base.OnReleased();
        }

        [Client]
        private void OnSelect()
        {
            Cmd_Select();
        }

        [Client]
        private void OnUnselect()
        {
            Cmd_Unselect();
        }

        [Command(requiresAuthority = false)]
        private void Cmd_Select()
        {
            boardPlaceableManager.CachePlaceableConfig(myFacilityConfig.PlaceableConfig);
        }

        [Command(requiresAuthority = false)]
        private void Cmd_Unselect()
        {
            boardPlaceableManager.ClearCachedPlaceableConfig();
        }
    }
}
