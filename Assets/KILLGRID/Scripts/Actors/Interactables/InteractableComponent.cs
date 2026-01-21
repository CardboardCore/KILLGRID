using System;
using Attic.Mirror.Actors.Components;
using HighlightPlus;
using Mirror;
using UnityEngine;

namespace KILLGRID.Actors.Interactables
{
    public enum InteractableType
    {
        None,
        MemoryBank,
        Tile,
    }

    [Serializable]
    public class InteractableConfig
    {
        [SerializeField] private bool immediateUnselect;
        [SerializeField] private InteractableType interactableType;
        [SerializeField] private HighlightEffect highlightEffect;
        [SerializeField] private HighlightProfile hoverProfile;
        [SerializeField] private HighlightProfile selectProfile;

        public bool ImmediateUnselect => immediateUnselect;
        public InteractableType InteractableType => interactableType;
        public HighlightEffect HighlightEffect => highlightEffect;
        public HighlightProfile HoverProfile => hoverProfile;
        public HighlightProfile SelectProfile => selectProfile;
    }

    public class InteractableComponent : ActorComponent
    {
        [SerializeField] private InteractableConfig interactableConfig;

        private bool isSelected;

        public InteractableConfig InteractableConfig => interactableConfig;

        public event Action HoverEnterEvent;
        public event Action HoverExitEvent;
        public event Action SelectEvent;
        public event Action UnSelectEvent;

        [Command(requiresAuthority = false)]
        private void Cmd_SetHighlight(bool isHighlighted) { Rpc_SetHighlight(isHighlighted); }

        [Command(requiresAuthority = false)]
        private void Cmd_Select() { Rpc_Select(); }

        [Command(requiresAuthority = false)]
        private void Cmd_UnSelect() { Rpc_UnSelect(); }

        [ClientRpc]
        private void Rpc_SetHighlight(bool isHighlighted) { ShowHighlight(false, isHighlighted); }

        [ClientRpc]
        private void Rpc_Select() { Select(false); }

        [ClientRpc]
        private void Rpc_UnSelect() { UnSelect(false); }

        [Client]
        public void ShowHighlight(bool callCommand, bool isHighlighted)
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

            if (callCommand)
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
        public void Select(bool callCommand)
        {
            if (isSelected)
            {
                return;
            }

            if (!interactableConfig.ImmediateUnselect)
            {
                isSelected = true;

                interactableConfig.HighlightEffect.ProfileLoad(interactableConfig.SelectProfile);
                interactableConfig.HighlightEffect.highlighted = true;
            }

            if (callCommand)
            {
                SelectEvent?.Invoke();
                Cmd_Select();
            }
        }

        [Client]
        public void UnSelect(bool callCommand)
        {
            if (!isSelected)
            {
                return;
            }

            isSelected = false;

            interactableConfig.HighlightEffect.highlighted = false;

            if (callCommand)
            {
                UnSelectEvent?.Invoke();
                Cmd_UnSelect();
            }
        }
    }
}
