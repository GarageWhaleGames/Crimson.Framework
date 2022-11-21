using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components;
using Assets.Crimson.Core.Components.Tags;
using Assets.Crimson.Core.Components.Targets;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.Systems
{
	public class AimAssistSystem : ComponentSystem
	{
		private EntityQuery _aimQuery;
		private EntityQuery _visibleEnemy;

		protected override void OnCreate()
		{
			_aimQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityAimAssist>(),
				ComponentType.ReadWrite<AimData>(),
				ComponentType.ReadOnly<PlayerInputData>()
				);
			_visibleEnemy = GetEntityQuery(
				ComponentType.ReadOnly<EnemyData>(),
				ComponentType.Exclude<InvisibleTag>(),
				ComponentType.Exclude<DeadActorTag>()
				);
		}

		protected override void OnUpdate()
		{
			Entities.With(_aimQuery).ForEach(
				(Entity entity, AbilityAimAssist ability, ref AimData aimData, ref PlayerInputData inputData) =>
				{
					var aimPosition = ability.AimPositionByInput(inputData);
					aimData.RealPosition = aimPosition;
					var minimalAimDistance = float.MaxValue;
					var minimalSourceDistance = ability.AimRange;
					var targetData = new EnemyTargetData
					{
						Position = aimPosition
					};
					Entities.With(_visibleEnemy).ForEach(
						(Actor actor, ref EnemyData enemyData) =>
						{
							var enemyPosition = actor.transform.position + (Vector3)enemyData.Offset;
							if (Vector3.Distance(enemyPosition, ability.transform.position) <= ability.AimRange)
							{
								var distanceToAim = math.distance(enemyPosition, aimPosition);
								var sourceDistance = math.distance(enemyPosition, ability.transform.position);
								if (ability.LockRange > distanceToAim
									&& sourceDistance < minimalSourceDistance)
								{
									targetData.Position = enemyPosition;
									targetData.Rotation = actor.transform.rotation;
									minimalAimDistance = distanceToAim;
									minimalSourceDistance = sourceDistance;
									targetData.Entity = actor.ActorEntity;
								}
							}
						});
					aimData.Target = targetData.Entity;
					if (EntityManager.HasComponent<EnemyTargetData>(entity))
					{
						EntityManager.SetComponentData(entity, targetData);
					}
					else
					{
						EntityManager.AddComponentData(entity, targetData);
					}
					aimData.LockedPosition = targetData.Position;
					EntityManager.SetComponentData(entity, aimData);
				});
		}
	}
}