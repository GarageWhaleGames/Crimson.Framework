using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Crimson.Core.Common
{
	public struct MoveData : IComponentData
	{
		public float Velocity;
		public float3 EndPosition;
		public float PositionThreshold;
		public bool IgnoreRaycast;
	}
}