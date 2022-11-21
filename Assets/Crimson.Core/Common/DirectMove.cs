using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Crimson.Core.Common
{
	public struct DirectMoveData : IComponentData
	{
		public float Duration;
		public float Velocity;
		public float3 Direction;
	}
}