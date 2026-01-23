using Attic.DI;

namespace KILLGRID.Input
{
    [Injectable]
    public class InputManager
    {
        private GameInputActions inputActions;

        public GameInputActions.PlayerActions Player => inputActions.Player;
        public GameInputActions.CheatsActions Cheats => inputActions.Cheats;

        public InputManager()
        {
            inputActions = new GameInputActions();
            inputActions.Enable();

            Player.Disable();
            Cheats.Disable();
        }

        ~InputManager()
        {
            if (inputActions != null && inputActions.asset != null)
            {
                inputActions.Disable();
                inputActions.Dispose();
                inputActions = null;
            }
        }
    }
}
