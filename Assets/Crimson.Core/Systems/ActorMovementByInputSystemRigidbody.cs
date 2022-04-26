using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorMovementByInputSystemRigidbody : ComponentSystem
	{
		private EntityQuery _query;

		protected override void OnCreate()
		{
			_query = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<MoveByInputData>(),
				ComponentType.ReadOnly<ActorMovementData>(),
				ComponentType.ReadOnly<Rigidbody>(),
				ComponentType.Exclude<StopMovementData>(),
				ComponentType.Exclude<DeadActorTag>());
		}

		protected override void OnUpdate()
		{
			var dt = Time.fixedDeltaTime;
			var t = (float)Time.ElapsedTime;

			Entities.With(_query).ForEach(
				(Entity entity, Rigidbody rigidBody, ref ActorMovementData movement) =>
				{
					if (rigidBody == null) return;

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

					var go = rigidBody.gameObject;
					var position = go.transform.position;
					var newPos = position + movementDelta;

					rigidBody.MovePosition(newPos);
				}
			);
		}
	}
}