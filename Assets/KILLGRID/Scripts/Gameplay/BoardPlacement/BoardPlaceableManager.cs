using Attic.DI;
using Attic.Mirror;
using KILLGRID.Actors.HexGrid;
using KILLGRID.Actors.Placeables;
using Mirror;

namespace KILLGRID.Gameplay.BoardPlacement
{
    [Injectable]
    public class BoardPlaceableManager : AtticNetworkBehaviour
    {
        private BoardPlaceableActor boardPlaceableActor;

        public BoardPlaceableConfig CurrentPlaceableConfig { get; private set; }

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        [Server]
        public void CachePlaceableConfig(BoardPlaceableConfig config)
        {
            CurrentPlaceableConfig = config;

            boardPlaceableActor = Instantiate(config.BoardPlaceableActorPrefab);
            NetworkServer.Spawn(boardPlaceableActor.gameObject);

            boardPlaceableActor.HideVisuals();
        }

        [Server]
        public void ClearCachedPlaceableConfig()
        {
            if (boardPlaceableActor)
            {
                boardPlaceableActor = null;
            }

            CurrentPlaceableConfig = null;
        }

        [Command(requiresAuthority = false)]
        public void Cmd_ShowPlaceableAtTile(HexTileActor hexTileActor)
        {
            if (!boardPlaceableActor || CurrentPlaceableConfig == null)
            {
                return;
            }

            boardPlaceableActor.ShowAsHologram();
            boardPlaceableActor.transform.position = hexTileActor.transform.position;
        }

        [Command(requiresAuthority = false)]
        public void Cmd_HidePlaceable()
        {
            if (!boardPlaceableActor || CurrentPlaceableConfig == null)
            {
                return;
            }

            boardPlaceableActor.HideVisuals();
        }
    }
}
