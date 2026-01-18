using System.Collections.Generic;
using Attic.Mirror.Actors.Components;
using Attic.DI;

namespace Attic.Mirror.Actors
{
    [Injectable]
    public class ActorRegistry
    {
        private readonly Dictionary<Actor, uint> actorToNetId = new Dictionary<Actor, uint>();

        public void RegisterActor(Actor actor)
        {
            actorToNetId.Add(actor, actor.netId);
        }

        public void UnregisterActor(Actor actor)
        {
            if (actorToNetId.ContainsKey(actor))
            {
                actorToNetId.Remove(actor);
            }
        }

        public Actor GetActor(uint netId)
        {
            foreach (KeyValuePair<Actor, uint> pair in actorToNetId)
            {
                if (pair.Value == netId)
                {
                    return pair.Key;
                }
            }

            return null;
        }

        public Actor[] GetAllActors()
        {
            List<Actor> actors = new List<Actor>();
            foreach (KeyValuePair<Actor, uint> keyValuePair in actorToNetId)
            {
                actors.Add(keyValuePair.Key);
            }

            return actors.ToArray();
        }

        public Actor[] GetLocallyOwnedActors()
        {
            List<Actor> actors = new List<Actor>();
            foreach (KeyValuePair<Actor, uint> keyValuePair in actorToNetId)
            {
                if (keyValuePair.Key.isOwned)
                {
                    actors.Add(keyValuePair.Key);
                }
            }

            return actors.ToArray();
        }

        public T FindNearestActor<T>(UnityEngine.Vector3 position) where T : Actor
        {
            float minDistance = float.MaxValue;
            T nearestActor = null;

            foreach (KeyValuePair<Actor, uint> pair in actorToNetId)
            {
                float distance = UnityEngine.Vector3.Distance(pair.Key.transform.position, position);

                if (distance >= minDistance)
                {
                    continue;
                }

                T actor = pair.Key as T;

                if (actor == null)
                {
                    continue;
                }

                minDistance = distance;
                nearestActor = actor;
            }

            return nearestActor;
        }

        public Actor FindNearestActor<T1, T2>(UnityEngine.Vector3 position) where T1 : Actor where T2 : Actor
        {
            float minDistance = float.MaxValue;
            Actor nearestActor = null;

            foreach (KeyValuePair<Actor, uint> pair in actorToNetId)
            {
                float distance = UnityEngine.Vector3.Distance(pair.Key.transform.position, position);

                if (distance >= minDistance)
                {
                    continue;
                }

                if (pair.Key is T1 || pair.Key is T2)
                {
                    minDistance = distance;
                    nearestActor = pair.Key;
                }
            }

            return nearestActor;
        }

        public Actor FindNearestActorWithComponent<T>(UnityEngine.Vector3 position) where T : ActorComponent
        {
            float minDistance = float.MaxValue;
            Actor nearestActor = null;

            foreach (KeyValuePair<Actor, uint> pair in actorToNetId)
            {
                float distance = UnityEngine.Vector3.Distance(pair.Key.transform.position, position);

                if (distance >= minDistance)
                {
                    continue;
                }

                if (pair.Key.GetActorComponent<T>())
                {
                    minDistance = distance;
                    nearestActor = pair.Key;
                }
            }

            return nearestActor;
        }

        public T FindRandomActorWithComponent<T>() where T : ActorComponent
        {
            List<Actor> actors = new List<Actor>();

            foreach (KeyValuePair<Actor, uint> pair in actorToNetId)
            {
                if (pair.Key.GetActorComponent<T>())
                {
                    actors.Add(pair.Key);
                }
            }

            if (actors.Count == 0)
            {
                return null;
            }

            Actor actor = actors[UnityEngine.Random.Range(0, actors.Count)];

            return actor.GetActorComponent<T>();
        }

        public T[] FindAllActorsWithComponent<T>() where T : ActorComponent
        {
            List<T> components = new List<T>();

            foreach (KeyValuePair<Actor, uint> pair in actorToNetId)
            {
                T component = pair.Key.GetActorComponent<T>();

                if (component)
                {
                    components.Add(component);
                }
            }

            return components.ToArray();
        }
    }
}
