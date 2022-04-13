using Unity.Entities;

namespace Assets.Crimson.Core.Common.Magnets
{
	public struct MagnetSnapData : IComponentData
	{
		public Entity Target;
		public float SnapSpeed;
		public float DistanceThreshold;
		public float RotationTheshold;
	}
}