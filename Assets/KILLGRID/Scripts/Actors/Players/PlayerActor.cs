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

        protected override void OnInjected()
        {
            base.OnInjected();

            viewObject.SetActive(!isLocalPlayer);
        }

        [TargetRpc]
        private void Rpc_EnableInput(NetworkConnectionToClient target)
        {
            inputManager.Player.Enable();
        }

        [Server]
        public void EnableInput()
        {
            Rpc_EnableInput(connectionToClient);
        }
    }
}
