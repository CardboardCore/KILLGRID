using KILLGRID.Actors.Players;
using KILLGRID.Actors.TableButtons;

namespace KILLGRID.Actors.Interactables.Requirements
{
    public class MemoryBankInteractRequirement : InteractRequirement
    {
        public override bool CanInteract(PlayerActor interactingPlayer)
        {
            PlayerMemoryBankComponent playerMemoryBankComponent = interactingPlayer.GetComponent<PlayerMemoryBankComponent>();

            if (!playerMemoryBankComponent)
            {
                return false;
            }

            MemoryBankComponent memoryBankComponent = GetComponent<MemoryBankComponent>();

            if (!memoryBankComponent)
            {
                return false;
            }

            bool isOwnedByPlayer = playerMemoryBankComponent.IsOwnedByPlayer(memoryBankComponent.netId);

            if (!isOwnedByPlayer)
            {
                return false;
            }

            PlayerEnergyComponent playerEnergyComponent = interactingPlayer.GetComponent<PlayerEnergyComponent>();

            if (!playerEnergyComponent)
            {
                return false;
            }

            return playerEnergyComponent.TotalEnergy >= memoryBankComponent.PlaceableConfig.Cost;
        }
    }
}
