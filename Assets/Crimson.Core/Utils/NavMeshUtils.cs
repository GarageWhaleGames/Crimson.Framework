using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Utils
{
	public static class NavMeshUtils
	{
		public static Vector3[] CalculatePositionsOnCircle(Vector3 sourcePosition, float distance, int points)
		{
			var angleRad = Mathf.Sin(Mathf.PI / points);
			var smallRadius = angleRad * distance;
			var angleDegree = 360 / points;
			var initPosition = Vector3.forward * distance;
			var results = new Vector3[points];
			for (var i = 0; i < points; i++)
			{
				var position = initPosition + sourcePosition;
				results[i] = NavMesh.SamplePosition(position, out var hit, smallRadius, NavMesh.AllAreas)
					? hit.position
					: position;
				initPosition = Quaternion.AngleAxis(angleDegree, Vector3.up) * initPosition;
			}
			return results;
		}

		public static float2 GetVelocity(this NavMeshAgent agent)
		{
			var hasIsArrived = agent.isOnNavMesh && agent.remainingDistance <= agent.stoppingDistance;
			if (hasIsArrived)
			{
				return float2.zero;
			}
			var direction = math.normalizesafe(new float2(agent.velocity.x, agent.velocity.z), float2.zero);
			return direction;
		}

		public static float Length(this NavMeshPath path)
		{
			var length = 0.0f;

			if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
			{
				for (var i = 1; i < path.corners.Length; ++i)
				{
					length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
				}
			}

			return length;
		}

		public static bool TryGetFirstReachable(this NavMeshAgent agent, Vector3[] positions, out Vector3 resultPosition)
		{
			var result = false;
			resultPosition = Vector3.zero;

			var path = new NavMeshPath();
			for (var i = 0; i < positions.Length; i++)
			{
				var position = positions[i];
				if (agent.CalculatePath(position, path))
				{
					result = true;
					resultPosition = position;
				}
			}

			return result;
		}
	}
}