using Attic.Mirror.Actors;
using Attic.DI;
using Mirror;
using UnityEngine;

namespace Attic.Mirror.Factories
{
    public class ActorFactory : CardboardCoreBehaviour
    {
        [SerializeField] private ActorFactoryConfig actorFactoryConfig;

        private Actor previouslySpawnedActor;

        protected override void OnInjected()
        {

        }

        protected override void OnReleased()
        {

        }

        /// <summary>
        /// Spawns a new <see cref="Actor"/> instance using a random prefab from the <see cref="ActorFactoryConfig"/>.
        /// If <paramref name="avoidPreviouslySpawned"/> is true, avoids spawning the same prefab as the last spawned actor.
        /// The spawned actor is instantiated at the factory's position and rotation, and registered with the network server.
        /// </summary>
        /// <param name="avoidPreviouslySpawned">
        /// If true, avoids spawning the same prefab as the previously spawned actor.
        /// </param>
        /// <returns>
        /// The spawned <see cref="Actor"/> instance, or <c>null</c> if no prefab is available.
        /// </returns>
        public Actor Spawn(bool avoidPreviouslySpawned = false)
        {
            Actor randomPrefab = avoidPreviouslySpawned ? actorFactoryConfig.GetRandomPrefab(previouslySpawnedActor) : actorFactoryConfig.GetRandomPrefab();

            if (randomPrefab == null)
            {
                Debug.LogError("No prefab available to spawn.");
                return null;
            }

            Actor instance = Instantiate(randomPrefab, transform.position, transform.rotation);
            NetworkServer.Spawn(instance.gameObject);

            previouslySpawnedActor = instance;

            return instance;
        }
    }
}
