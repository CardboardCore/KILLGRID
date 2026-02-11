using System;
using System.Collections.Generic;

namespace KILLGRID.Gameplay.HexGrid
{
    public class SimplePriorityQueue<T>
    {
        private readonly List<(T item, int priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].priority < elements[bestIndex].priority)
                    bestIndex = i;
            }

            T bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    public static class HexGridUtils
    {
        // Even-q offset for flat-top hex grid
        public static readonly (int x, int y)[] EvenQOffsets = new (int, int)[]
        {
            (0, 1),   // N
            (1, 0),   // NE
            (1, -1),  // SE
            (0, -1),  // S
            (-1, -1), // SW
            (-1, 0)   // NW
        };

        // Odd-q offset for flat-top hex grid
        public static readonly (int x, int y)[] OddQOffsets = new (int, int)[]
        {
            (0, 1),  // N
            (1, 1),  // NE
            (1, 0),  // SE
            (0, -1), // S
            (-1, 0), // SW
            (-1, 1)  // NW
        };
    }

    public class HexGridPathfinder
    {
        private static (int x, int y)[] GetNeighbors(int q, int r)
        {
            (int x, int y)[] offsets = (q & 1) == 0 ? HexGridUtils.EvenQOffsets : HexGridUtils.OddQOffsets;

            List<(int x, int y)> result = new List<(int x, int y)>(6);

            foreach ((int dx, int dy) in offsets)
            {
                result.Add((q + dx, r + dy));
            }

            return result.ToArray();
        }

        public static (int x, int y, int z) OffsetToCube(int q, int r)
        {
            int x = q;
            int z = r - (q - (q & 1)) / 2;
            int y = -x - z;
            return (x, y, z);
        }

        public static int CubeDistance((int x, int y, int z) a, (int x, int y, int z) b)
        {
            return Math.Max(Math.Abs(a.x - b.x), Math.Max(Math.Abs(a.y - b.y), Math.Abs(a.z - b.z)));
        }

        public static int HexDistance((int q, int r) a, (int q, int r) b)
        {
            (int x, int y, int z) ac = OffsetToCube(a.q, a.r);
            (int x, int y, int z) bc = OffsetToCube(b.q, b.r);
            return CubeDistance(ac, bc);
        }

        public static List<(int q, int r)> FindPath((int q, int r) dimensions,
                                                    (int q, int r) start,
                                             (int q, int r) goal,
                                             Func<int, int, bool> isBlocked)
        {
            SimplePriorityQueue<(int q, int r)> open = new SimplePriorityQueue<(int q, int r)>();
            Dictionary<(int q, int r), (int q, int r)> cameFrom = new Dictionary<(int q, int r), (int q, int r)>();
            Dictionary<(int q, int r), int> gScore = new Dictionary<(int q, int r), int>();

            gScore[start] = 0;
            open.Enqueue(start, HexDistance(start, goal));

            while (open.Count > 0)
            {
                (int q, int r) current = open.Dequeue();

                if (current.Equals(goal))
                    return ReconstructPath(cameFrom, current);

                foreach ((int nq, int nr) in GetNeighbors(current.q, current.r))
                {
                    if (nq < 0 || nq >= dimensions.q || nr < 0 || nr >= dimensions.r)
                    {
                        continue; // out of bounds
                    }

                    if (isBlocked != null && isBlocked(nq, nr))
                    {
                        continue;
                    }

                    (int nq, int nr) neighbor = (nq, nr);
                    int tentative = gScore[current] + 1;

                    if (!gScore.TryGetValue(neighbor, out int existing) || tentative < existing)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative;

                        int f = tentative + HexDistance(neighbor, goal);
                        open.Enqueue(neighbor, f);
                    }
                }
            }

            return null; // no path found
        }

        private static List<(int q, int r)> ReconstructPath(
            Dictionary<(int q, int r), (int q, int r)> cameFrom,
            (int q, int r) current)
        {
            List<(int q, int r)> path = new List<(int q, int r)> { current };

            while (cameFrom.TryGetValue(current, out (int q, int r) prev))
            {
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
