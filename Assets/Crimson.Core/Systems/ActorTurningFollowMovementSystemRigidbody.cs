using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorTurningFollowMovementSystemRigidbody : ComponentSystem
	{
		private EntityQuery _query;

		protected override void OnCreate()
		{
			_query = GetEntityQuery(
				ComponentType.ReadOnly<ActorMovementData>(),
				ComponentType.ReadOnly<ActorRotationFollowMovementData>(),
				ComponentType.ReadOnly<Rigidbody>(),
				ComponentType.Exclude<StopRotationData>());
		}

		protected override void OnUpdate()
		{
			var dt = Time.fixedDeltaTime;

			Entities.With(_query).ForEach((Entity entity,
										   Rigidbody rigidBody,
										   ref ActorMovementData movement,
										   ref ActorRotationFollowMovementData rotation) =>
			{
				if (rigidBody == null)
				{
					return;
				}

				var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

				var direction = Vector3.zero;
				if (dstManager.HasComponent(entity, typeof(ActorEvaluateAimingAnimData)))
				{
					var c = dstManager.GetComponentData<ActorEvaluateAimingAnimData>(entity);
					if (c.AimingActive)
					{
						var aimData = dstManager.GetComponentData<AimingData>(entity);
						direction = aimData.Direction;
					}
					else
					{
						direction = new Vector3(movement.MovementCache.x, movement.MovementCache.y, movement.MovementCache.z);
					}
				}
				else
				{
					direction = new Vector3(movement.MovementCache.x, movement.MovementCache.y, movement.MovementCache.z);
				}

				if (direction == Vector3.zero)
				{
					return;
				}

				var rot = rigidBody.rotation;
				var newRot = Quaternion.LookRotation(Vector3.Normalize(direction));
				if (newRot == rot)
				{
					return;
				}

				rigidBody.MoveRotation(Quaternion.Lerp(rot, newRot, dt * rotation.RotationSpeed));
			});
		}
	}
}