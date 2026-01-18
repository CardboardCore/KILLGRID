using Attic.DI;
using Attic.StateMachines;
using KILLGRID.Input;

namespace KILLGRID.Gameplay.StateMachines.States
{
    public class ActiveGameState : State
    {
        [Inject] private InputManager inputManager;

        protected override void OnEnter()
        {
            inputManager.Player.Enable();
        }

        protected override void OnExit()
        {
            inputManager.Player.Disable();
        }
    }
}
