using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorForceMovementSystem : ComponentSystem
	{
		private EntityQuery _query;

		protected override void OnCreate()
		{
			_query = GetEntityQuery(
				ComponentType.ReadOnly<AbilityForceMovement>(),
				ComponentType.ReadOnly<ActorMovementData>(),
				ComponentType.ReadOnly<ActorForceMovementData>(),
				ComponentType.Exclude<StopMovementData>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_query).ForEach(
				(Entity entity, AbilityForceMovement forceMovement, ref ActorForceMovementData data,
					ref ActorMovementData movement) =>
				{
					switch (data.MoveDirection)
					{
						case MoveDirection.SpawnerForward:
							if (data.stopGuiding || forceMovement.Actor.Spawner == null) return;
							data.ForwardVector = forceMovement.Spawner.forward;
							data.ForwardVector += (float3)forceMovement.Spawner.TransformVector(data.OffsetDirection);
							data.stopGuiding = true;
							break;

						case MoveDirection.UseDirection:
							if (!data.stopGuiding && data.CompensateSpawnerRotation)
							{
								data.ForwardVector -= (float3)forceMovement.Spawner.forward;
								data.stopGuiding = true;
							}
							break;

						case MoveDirection.GuidedBySpawner:
							if (forceMovement.Actor.Spawner == null) return;
							data.ForwardVector = forceMovement.Spawner.forward;
							break;

						case MoveDirection.SelfForward:
							data.ForwardVector = forceMovement.transform.forward;
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					movement.Input = data.ForwardVector;
					if (data.UseVariance)
					{
						var vector = data.Variance.GeneratePosition();
						var rotation = data.Variance.GenerateRotation();
						movement.Input += (float3)vector;
						movement.Input = rotation * movement.Input;
						movement.MovementCache = movement.Input;
					}
				}
			);
		}
	}
}