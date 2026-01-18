using Attic.Mirror.Actors;
using UnityEngine;

namespace Attic.Mirror.Factories
{
    [CreateAssetMenu(fileName = "ActorFactoryConfig", menuName = "LinxCore/Actors/ActorFactoryConfig", order = 1)]
    public class ActorFactoryConfig : ScriptableObject
    {
        [SerializeField] private Actor[] prefabs;

        public Actor GetRandomPrefab(params Actor[] filter)
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError("No prefabs configured in FactoryConfig.");
                return null;
            }

            if (filter != null && filter.Length > 0)
            {
                // Filter out prefabs that are in the filter array
                Actor[] filteredPrefabs = System.Array.FindAll(prefabs, prefab => !System.Array.Exists(filter, f => f == prefab));

                if (filteredPrefabs.Length == 0)
                {
                    Debug.LogError("No prefabs available after filtering.");
                    return null;
                }

                return filteredPrefabs[Random.Range(0, filteredPrefabs.Length)];
            }

            return prefabs[Random.Range(0, prefabs.Length)];
        }
    }
}
