using Attic.Cameras.VirtualCameras;
using Attic.DI;
using KILLGRID.Input;
using UnityEngine;
using Attic.Mirror.Actors;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerActor : Actor
    {
        [Inject] private InputManager inputManager;
        [Inject] private VirtualCameraManager virtualCameraManager;

        [SerializeField] private GameObject viewObject;

        [SyncVar] private int playerIndex;

        public int PlayerIndex => playerIndex;
        public PlayerInteractComponent PlayerInteractComponent { get; private set; }

        protected override void OnInjected()
        {
            base.OnInjected();

            viewObject.SetActive(!isLocalPlayer);

            PlayerInteractComponent = GetComponent<PlayerInteractComponent>();
        }

        [TargetRpc]
        private void Rpc_EnableInput(NetworkConnectionToClient target)
        {
            inputManager.Player.Enable();
        }

        [Server]
        public void SetPlayerIndex(int playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        [Server]
        public void EnableInput()
        {
            Rpc_EnableInput(connectionToClient);
        }
    }
}
