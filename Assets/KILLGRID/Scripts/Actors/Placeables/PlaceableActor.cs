using System;
using Attic.Mirror.Actors;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Placeables
{

    public class PlaceableActor : Actor
    {
        [SerializeField] private GameObject greenHoloView;
        [SerializeField] private GameObject redHoloView;
        [SerializeField] private GameObject normalView;

        private bool isSpawned;
        private event Action spawnEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            HideVisuals(false);

            spawnEvent?.Invoke();
            spawnEvent = null;

            isSpawned = true;
        }

        [Command(requiresAuthority = false)]
        private void Cmd_HideVisuals() { Rpc_HideVisuals(); }

        [Command(requiresAuthority = false)]
        private void Cmd_ShowAsGreenHologram() { Rpc_ShowAsGreenHologram(); }

        [Command(requiresAuthority = false)]
        private void Cmd_ShowAsRedHologram() { Rpc_ShowAsRedHologram(); }

        [Command(requiresAuthority = false)]
        private void Cmd_ShowAsNormal() { Rpc_ShowAsNormal(); }

        [ClientRpc]
        private void Rpc_HideVisuals() { HideVisuals(false); }

        [ClientRpc]
        private void Rpc_ShowAsGreenHologram() { ShowAsGreenHologram(false); }

        [ClientRpc]
        private void Rpc_ShowAsRedHologram() { ShowAsRedHologram(false); }

        [ClientRpc]
        private void Rpc_ShowAsNormal() { ShowAsNormal(false); }

        [Client]
        public void WhenSpawned(Action callback)
        {
            if (isSpawned)
            {
                callback();
            }
            else
            {
                spawnEvent += callback;
            }
        }

        [Client]
        public void HideVisuals(bool callCommand)
        {
            greenHoloView.SetActive(false);
            normalView.SetActive(false);

            if (callCommand)
            {
                Cmd_HideVisuals();
            }
        }

        [Client]
        public void ShowAsGreenHologram(bool callCommand)
        {
            greenHoloView.SetActive(true);
            redHoloView.SetActive(false);
            normalView.SetActive(false);

            if (callCommand)
            {
                Cmd_ShowAsGreenHologram();
            }
        }

        [Client]
        public void ShowAsRedHologram(bool callCommand)
        {
            redHoloView.SetActive(true);
            greenHoloView.SetActive(false);
            normalView.SetActive(false);

            if (callCommand)
            {
                Cmd_ShowAsRedHologram();
            }
        }

        [Client]
        public void ShowAsNormal(bool callCommand)
        {
            greenHoloView.SetActive(false);
            redHoloView.SetActive(false);
            normalView.SetActive(true);

            if (callCommand)
            {
                Cmd_ShowAsNormal();
            }
        }
    }
}
