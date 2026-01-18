using Attic.Mirror.Actors;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{
    public class BoardPlaceableActor : Actor
    {
        [SerializeField] private GameObject holoView;
        [SerializeField] private GameObject normalView;

        protected override void OnInjected()
        {
            base.OnInjected();

            holoView.SetActive(false);
            normalView.SetActive(false);
        }

        [ClientRpc]
        private void Rpc_HideVisuals()
        {
            holoView.SetActive(false);
            normalView.SetActive(false);
        }

        [ClientRpc]
        private void Rpc_ShowAsHologram()
        {
            holoView.SetActive(true);
            normalView.SetActive(false);
        }

        [ClientRpc]
        private void Rpc_ShowAsNormal()
        {
            holoView.SetActive(false);
            normalView.SetActive(true);
        }

        [Server]
        public void HideVisuals()
        {
            Rpc_HideVisuals();
        }

        [Server]
        public void ShowAsHologram()
        {
            Rpc_ShowAsHologram();
        }

        [Server]
        public void ShowAsNormal()
        {
            Rpc_ShowAsNormal();
        }
    }
}
