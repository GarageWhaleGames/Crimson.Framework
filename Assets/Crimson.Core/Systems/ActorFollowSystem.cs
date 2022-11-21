using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Systems
{
	public class ActorFollowSystem : ComponentSystem
	{
		private EntityQuery _actorFollowQuery;

		protected override void OnCreate()
		{
			_actorFollowQuery = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<AbilityFollowActor>(),
				ComponentType.ReadOnly<ActorFollowData>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_actorFollowQuery).ForEach(
				(ref ActorFollowData followData, AbilityFollowActor abilityFollow, Transform transform) =>
				{
					var targetTransform = abilityFollow.AbilityTarget.Target.transform;
					var direction = transform.position - targetTransform.position;
					var thresholdPostion = direction.normalized * followData.PositionThreshold;

					transform.position = targetTransform.position + thresholdPostion;
				});
		}
	}
}