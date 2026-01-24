using Attic.DI;
using Attic.Mirror.Actors.Components;
using Attic.Utilities;
using KILLGRID.Actors.Interactables;
using KILLGRID.Gameplay.Placeables;
using KILLGRID.Gameplay.Tables;
using Mirror;
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
        private TableSpot myTableSpot;

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

        [Command(requiresAuthority = false)]
        private void Cmd_Consume()
        {
            myTableSpot.ClearSpot();

            NetworkServer.Destroy(gameObject);
        }

        [Server]
        public void CacheTableSpot(TableSpot tableSpot)
        {
            myTableSpot = tableSpot;
        }

        [Client]
        public void RequestConsume()
        {
            Cmd_Consume();
            // TODO: Change to playing animations and use "callCommand" chain to make it feel direct
        }
    }
}
