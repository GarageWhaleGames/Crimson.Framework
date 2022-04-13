using UnityEngine;
using UnityEngine.AI;

namespace Crimson.Core.Utils
{
	public static class NavMeshRandomPointUtil
	{
		public static Vector3 GetRandomLocation(float limitDistance = 2.0f)
		{
			while (true)
			{
				NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

				int maxIndices = navMeshData.indices.Length - 3;

				int firstVertexSelected = Random.Range(0, maxIndices);
				int secondVertexSelected = Random.Range(0, maxIndices);

				Vector3 point;

				Vector3 firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
				Vector3 secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];

				if ((int)firstVertexPosition.x == (int)secondVertexPosition.x || (int)firstVertexPosition.z == (int)secondVertexPosition.z)
				{
					continue;
				}

				point = Vector3.Lerp(firstVertexPosition, secondVertexPosition,
					Random.Range(0.0f, 1f));

				if (NavMesh.SamplePosition(point, out var hit, limitDistance, NavMesh.AllAreas))
				{
					return hit.position;
				}
			}
		}
	}
}