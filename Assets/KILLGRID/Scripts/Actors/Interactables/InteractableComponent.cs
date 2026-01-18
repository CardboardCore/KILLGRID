using System;
using Attic.Mirror.Actors.Components;
using HighlightPlus;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Interactables
{
    [Serializable]
    public class InteractableConfig
    {
        [SerializeField] private HighlightEffect highlightEffect;
        [SerializeField] private HighlightProfile hoverProfile;
        [SerializeField] private HighlightProfile selectProfile;

        public HighlightEffect HighlightEffect => highlightEffect;
        public HighlightProfile HoverProfile => hoverProfile;
        public HighlightProfile SelectProfile => selectProfile;
    }

    public class InteractableComponent : ActorComponent
    {
        [SerializeField] private InteractableConfig interactableConfig;

        private bool isSelected;

        public event Action HoverEnterEvent;
        public event Action HoverExitEvent;
        public event Action SelectEvent;
        public event Action UnSelectEvent;

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

        [Command(requiresAuthority = false)]
        private void Cmd_Select()
        {
            Rpc_Select();
        }

        [ClientRpc]
        private void Rpc_Select( )
        {
            Select(false);
        }

        [Command(requiresAuthority = false)]
        private void Cmd_UnSelect()
        {
            Rpc_UnSelect();
        }

        [ClientRpc]
        private void Rpc_UnSelect()
        {
            UnSelect(false);
        }

        [Client]
        public void ShowHighlight(bool isLocal, bool isHighlighted)
        {
            if (isSelected)
            {
                return;
            }

            if (interactableConfig.HighlightEffect != null)
            {
                interactableConfig.HighlightEffect.ProfileLoad(interactableConfig.HoverProfile);
                interactableConfig.HighlightEffect.highlighted = isHighlighted;
            }

            if (isLocal)
            {
                if (isHighlighted)
                {
                    HoverEnterEvent?.Invoke();
                }
                else
                {
                    HoverExitEvent?.Invoke();
                }

                Cmd_SetHighlight(isHighlighted);
            }
        }

        [Client]
        public void Select(bool isLocal)
        {
            isSelected = true;

            interactableConfig.HighlightEffect.ProfileLoad(interactableConfig.SelectProfile);
            interactableConfig.HighlightEffect.highlighted = true;

            if (isLocal)
            {
                SelectEvent?.Invoke();
                Cmd_Select();
            }
        }

        [Client]
        public void UnSelect(bool isLocal)
        {
            isSelected = false;

            interactableConfig.HighlightEffect.highlighted = false;

            if (isLocal)
            {
                UnSelectEvent?.Invoke();
                Cmd_UnSelect();
            }
        }
    }
}
