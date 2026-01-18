using Attic.Utilities;

namespace Attic.Utils.Navigation
{
    using UnityEngine;
    using UnityEngine.AI;

    public class NavMeshUtils
    {
        public static Vector3 GetClosestPointOnNavMesh(Vector3 position, float maxDistance = 10.0f)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Handle the case where no point is found within the maxDistance
            Log.Warn("No point found on NavMesh within the specified distance.");
            return position;
        }
    }
}
