using Attic.DI;
using Attic.Mirror.Actors.Components;
using Attic.Utilities;
using KILLGRID.Actors.Interactables;
using KILLGRID.Gameplay.Placeables;
using TMPro;
using UnityEngine;

namespace KILLGRID.Actors.TableButtons
{
    public class MemoryBankComponent : ActorComponent
    {
        [Inject] private PlaceablesFactory placeablesFactory;

        [Header("References")]
        [SerializeField] private InteractableComponent interactableComponent;
        [SerializeField] private TextMeshPro nameText;
        [SerializeField] private TextMeshPro costText;

        [Header("Settings")]
        [SerializeField] private PlaceableType placeableType;

        private PlaceableConfig placeableConfig;

        public PlaceableConfig PlaceableConfig => placeableConfig;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!placeablesFactory.PlaceablesConfig.TryGetPlaceableConfig(placeableType, out placeableConfig))
            {
                Log.Error($"No placeable config found for type {placeableType}");
                return;
            }

            nameText.text = placeableConfig.Name;
            costText.text = placeableConfig.Cost.ToString();
        }
    }
}
