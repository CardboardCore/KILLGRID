using Attic.Cameras.VirtualCameras;
using Attic.DI;
using UnityEngine;
using Attic.Mirror.Actors;
using KILLGRID.Actors.PlayerMonitors;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerActor : Actor
    {
        [Inject] private VirtualCameraManager virtualCameraManager;

        [SerializeField] private GameObject viewObject;

        [SyncVar] private int playerIndex;

        public int PlayerIndex => playerIndex;
        public PlayerInteractComponent PlayerInteractComponent { get; private set; }
        public PlayerMonitorComponent MyMonitor { get; private set; }

        protected override void OnInjected()
        {
            base.OnInjected();

            viewObject.SetActive(!isLocalPlayer);

            PlayerInteractComponent = GetComponent<PlayerInteractComponent>();
        }

        [Server]
        public void SetPlayerIndex(int playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        [Client]
        public void SetMonitor(PlayerMonitorComponent monitor, string playerName)
        {
            MyMonitor = monitor;
            MyMonitor.SetPlayerName(playerName);
        }
    }
}
