using Unity.Entities;

namespace Assets.Crimson.Core.Common
{
	public struct ActorTargetData : IComponentData
	{
		public Entity TargetEntity;
	}
}