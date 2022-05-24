using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorFollowRotationSystem : ComponentSystem
	{
		private EntityQuery _lookQuery;
		private EntityQuery _query;

		protected override void OnCreate()
		{
			_query = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<ActorFollowRotationData>(),
				ComponentType.ReadOnly<RotateDirectlyData>(),
				ComponentType.Exclude<ActorNoFollowTargetRotationData>(),
				ComponentType.Exclude<StopRotationData>());

			_lookQuery = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<FollowLookRotationTag>(),
				ComponentType.ReadOnly<PlayerInputData>(),
				ComponentType.Exclude<StopRotationData>());
		}

        protected override void OnUpdate()
        {
            Entities.With(_query).ForEach(
                (Entity entity, AbilityFollowRotation follow, ref RotateDirectlyData rotation,
                    ref ActorFollowRotationData data) =>
                {
                    if (follow.target == null)
                    {
                        PostUpdateCommands.AddComponent<ActorNoFollowTargetRotationData>(entity);
                        if (follow.hideIfNoTarget) follow.gameObject.SetActive(false);
                        return;
                    }

					if (follow.hideIfNoTarget)
					{
						follow.gameObject.SetActive(true);
					}

					float3 targetEuler = follow.target.rotation.eulerAngles;

					var newRotation = follow.retainRotationOffset ? targetEuler - data.Origin : targetEuler;
					rotation.Rotation = newRotation;
				}
			);

			Entities.With(_lookQuery).ForEach(
				(ref PlayerInputData input, Transform transform) =>
				{
					var mouse = (Vector2)input.Mouse;
					var position = (Vector2)Camera.main.WorldToScreenPoint(transform.position);
					var angle = Mathf.Atan2(mouse.y - position.y, mouse.x - position.x) * (180 / Mathf.PI);

					transform.rotation = Quaternion.AngleAxis(-angle-90, Vector3.up);
				});
		}
	}
}