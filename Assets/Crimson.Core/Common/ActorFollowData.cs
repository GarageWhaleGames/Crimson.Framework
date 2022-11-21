using Unity.Entities;

namespace Assets.Crimson.Core.Common
{
	public struct ActorFollowData : IComponentData
	{
		public float PositionThreshold;
	}
}