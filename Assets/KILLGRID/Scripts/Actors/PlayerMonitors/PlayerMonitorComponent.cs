using System;
using Attic.Mirror.Actors.Components;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KILLGRID.Actors.PlayerMonitors
{
    public class PlayerMonitorComponent : ActorComponent
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private Button endTurnButton;

        public event Action PlayerEndTurnPressedEvent;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isClient)
            {
                return;
            }

            endTurnButton.onClick.AddListener(OnEndTurnButtonPressed);

            SetPlayerEnergy(0);
            SetIsPlayerTurn(false);
        }

        protected override void OnReleased()
        {
            if (isClient)
            {
                endTurnButton.onClick.RemoveListener(OnEndTurnButtonPressed);
            }

            base.OnReleased();
        }

        private void OnEndTurnButtonPressed()
        {
            PlayerEndTurnPressedEvent?.Invoke();
        }

        [Client]
        public void SetIsPlayerTurn(bool isPlayerTurn)
        {
            turnText.color = isPlayerTurn ? Color.green : Color.red;
            turnText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";
        }

        [Client]
        public void SetPlayerEnergy(int energy)
        {
            energyText.text = $"Energy: {energy}";
        }

        [Client]
        public void SetPlayerName(string playerName)
        {
            playerNameText.text = playerName;
        }

        [Client]
        public void SetState(string state)
        {
            stateText.text = $"{state}...";
        }

        [Client]
        public void SetRoundNumber(int roundNumber)
        {
            roundText.text = $"Round: {roundNumber}";
        }
    }
}
