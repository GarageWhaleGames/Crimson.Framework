using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Crimson.Core.Components.Targets
{
	public struct EnemyTargetData : IComponentData
	{
		public Entity Entity;
		public float3 Position;
		public quaternion Rotation;
	}
}