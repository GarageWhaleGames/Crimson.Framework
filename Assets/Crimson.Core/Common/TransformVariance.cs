using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Crimson.Core.Common
{
	[Serializable]
	public struct TransformVariance
	{
		public Vector3 PostionOffset;
		public Vector3 RotationOffset;

		internal float3 Generate()
		{
			var vector = GetRandomPosition(PostionOffset);
			var rotation = GetRandomRotation(RotationOffset);
			var eueler = rotation.eulerAngles;
			return math.mul(rotation, vector);
		}

		internal Vector3 GeneratePosition()
		{
			return GetRandomPosition(PostionOffset);
		}

		internal Quaternion GenerateRotation()
		{
			return GetRandomRotation(RotationOffset);
		}

		private Vector3 GetRandomPosition(Vector3 sourceOffset)
		{
			return new Vector3(
				Random.Range(-sourceOffset.x, sourceOffset.x)
				, Random.Range(-sourceOffset.y, sourceOffset.y)
				, Random.Range(-sourceOffset.z, sourceOffset.z));
		}

		private Quaternion GetRandomRotation(Vector3 sourceOffset)
		{
			return Quaternion.Euler(GetRandomPosition(sourceOffset));
		}
	}
}