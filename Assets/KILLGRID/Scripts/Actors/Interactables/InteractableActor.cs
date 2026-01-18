using System;
using Attic.Mirror.Actors;
using HighlightPlus;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Interactables
{
    [Serializable]
    public class InteractableConfig
    {
        [SerializeField] private HighlightEffect highlightEffect;

        public HighlightEffect HighlightEffect => highlightEffect;
    }

    public class InteractableActor : Actor
    {
        [SerializeField] private InteractableConfig interactableConfig;

        [Command(requiresAuthority = false)]
        private void Cmd_SetHighlight(bool isHighlighted)
        {
            Rpc_SetHighlight(isHighlighted);
        }

        [ClientRpc]
        private void Rpc_SetHighlight(bool isHighlighted)
        {
            ShowHighlight(false, isHighlighted);
        }

        [Client]
        public void ShowHighlight(bool isLocal, bool isHighlighted)
        {
            if (interactableConfig.HighlightEffect != null)
            {
                interactableConfig.HighlightEffect.highlighted = isHighlighted;
            }

            if (isLocal)
            {
                Cmd_SetHighlight(isHighlighted);
            }
        }
    }
}
