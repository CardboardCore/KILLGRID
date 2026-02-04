using Attic.Mirror.Actors.Components;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.PlayerMonitors
{
    public class MonitorHealthLightComponent : ActorComponent
    {
        [SerializeField] private Renderer lightRenderer;
        [SerializeField] private Material lightOffMaterial;
        [SerializeField] private Material lightOnMaterial;

        [SyncVar(hook = nameof(OnIsOnChanged))] private bool isOn;

        private Material currentMaterial;

        [Client]
        private void OnIsOnChanged(bool oldValue, bool newValue)
        {
            Material materialInstance = newValue ? lightOnMaterial : lightOffMaterial;
            Material newMaterial = new Material(materialInstance);

            // Assign new material first
            lightRenderer.material = newMaterial;

            // Then destroy the old one
            if (currentMaterial)
            {
                Destroy(currentMaterial);
            }

            currentMaterial = newMaterial;
        }

        [Server]
        public void SetIsLit(bool value)
        {
            isOn = value;
        }
    }
}
