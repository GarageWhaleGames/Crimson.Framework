using UnityEngine;

namespace Assets.Crimson.Core.Utils
{
	internal static class NavMeshUtils
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
				results[i] = UnityEngine.AI.NavMesh.SamplePosition(position, out var hit, smallRadius, UnityEngine.AI.NavMesh.AllAreas)
					? hit.position
					: position;
				initPosition = Quaternion.AngleAxis(angleDegree, Vector3.up) * initPosition;
			}
			return results;
		}

		public static float Length(this UnityEngine.AI.NavMeshPath path)
		{
			var length = 0.0f;

			if ((path.status != UnityEngine.AI.NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
			{
				for (var i = 1; i < path.corners.Length; ++i)
				{
					length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
				}
			}

			return length;
		}
	}
}