using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Tags
{
	public struct EnemyData : IComponentData
	{
		public float3 Offset;

		public EnemyData(Vector3 offset)
		{
			Offset = offset;
		}
	}
}