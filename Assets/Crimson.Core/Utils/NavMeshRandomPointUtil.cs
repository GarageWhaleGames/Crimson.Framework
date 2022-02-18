using UnityEngine;
using UnityEngine.AI;

namespace Crimson.Core.Utils
{
	public static class NavMeshRandomPointUtil
	{
		public static Vector3 GetRandomLocation()
		{
			while (true)
			{
				var navMeshData = NavMesh.CalculateTriangulation();

				var maxIndices = navMeshData.indices.Length - 3;

				var firstVertexSelected = Random.Range(0, maxIndices);
				var secondVertexSelected = Random.Range(0, maxIndices);

				Vector3 point;

				var firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
				var secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];

				if ((int)firstVertexPosition.x == (int)secondVertexPosition.x || (int)firstVertexPosition.z == (int)secondVertexPosition.z)
				{
					continue;
				}

				point = Vector3.Lerp(firstVertexPosition, secondVertexPosition,
					Random.Range(0.0f, 1f));

				if (NavMesh.SamplePosition(point, out var hit, 2.0f, NavMesh.AllAreas))
				{
					return hit.position;
				}
			}
		}
	}
}