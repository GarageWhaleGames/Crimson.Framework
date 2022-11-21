using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Crimson.Core.Common
{
	public struct ExplosionForceData : IComponentData
	{
		public float Force;
		public int ForceMode;
		public float3 Position;
		public float Radius;
		public float UpwardModifier;
	}
}