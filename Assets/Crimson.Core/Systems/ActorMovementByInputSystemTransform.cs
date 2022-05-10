using Assets.Crimson.Core.Components;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorMovementByInputSystemTransform : ComponentSystem
	{
		private EntityQuery _query;
		private EntityQuery _navQuery;

		protected override void OnCreate()
		{
			_query = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<MoveByInputData>(),
				ComponentType.ReadOnly<ActorMovementData>(),
				ComponentType.Exclude<Rigidbody>(),
				ComponentType.Exclude<StopMovementData>(),
				ComponentType.Exclude<DeadActorTag>());

			_navQuery = GetEntityQuery(
				ComponentType.ReadOnly<NavAgentProxy>(),
				ComponentType.ReadOnly<MoveByInputData>(),
				ComponentType.ReadOnly<ActorMovementData>(),
				ComponentType.Exclude<StopMovementData>(),
				ComponentType.Exclude<DeadActorTag>());
		}

		protected override void OnUpdate()
		{
			var dt = Time.fixedDeltaTime;
			var t = (float)Time.ElapsedTime;

			Entities.With(_query).ForEach(
				(Entity entity, Transform transform, ref ActorMovementData movement) =>
				{
					if (transform == null) return;
					var speed = movement.MovementSpeed;
					float multiplier;

					if (movement.Dynamics.useDynamics)
					{
						multiplier = MathUtils.ApplyDynamics(ref movement, t);
					}
					else
					{
						multiplier = 1f;
						movement.MovementCache = movement.Input;
					}

					var movementDelta = speed * dt * multiplier * movement.ExternalMultiplier *
										Vector3.ClampMagnitude(movement.MovementCache, 1f);

					if (movementDelta == Vector3.zero) return;

					var newPos = transform.position + movementDelta;
					transform.position = newPos;
				}
			);
		}
	}
}