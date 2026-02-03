using Attic.Mirror.Actors.Components;
using Mirror;

namespace KILLGRID.Actors.Players
{
    public class PlayerHealthComponent : ActorComponent
    {
        [SyncVar(hook = nameof(OnHealthChanged))] private int health;

        public int Health => health;

        protected override void OnInjected()
        {
            base.OnInjected();

            if (!isServer)
            {
                return;
            }

            health = 5;
        }

        [Client]
        private void OnHealthChanged(int oldHealth, int newHealth)
        {
            (Owner as PlayerActor)?.MyMonitor.SetPlayerHealth(newHealth);
        }

        [Server]
        public void TakeDamage(int damage)
        {
            int currentHealth = health;

            currentHealth -= damage;

            if (currentHealth < 0)
            {
                currentHealth = 0;
            }

            // Update health only once to avoid multiple SyncVar updates
            health = currentHealth;
        }
    }
}
