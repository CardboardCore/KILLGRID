using System.Collections.Generic;
using UnityEngine;

namespace Attic.Utils.Registries
{
    public abstract class Registry<TKey>
    {
        private List<TKey> keys = new List<TKey>();

        public IReadOnlyList<TKey> Keys => keys.AsReadOnly();

        public void Add(TKey key)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
            }
        }

        public void Remove(TKey key)
        {
            if (keys.Contains(key))
            {
                keys.Remove(key);
            }
        }

        public bool Contains(TKey key)
        {
            return keys.Contains(key);
        }

        public void Clear()
        {
            keys.Clear();
        }

        public abstract void GetNearest(Vector3 point, out TKey nearestKey);
    }
}
