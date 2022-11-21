using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components;
using Assets.Crimson.Core.Utils;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Systems
{
	public class TeleportSystem : ComponentSystem
	{
		private EntityQuery _aliveActorQuery;
		private EntityQuery _targetNavMeshQuery;
		private EntityQuery _targetTransformQuery;

		protected override void OnCreate()
		{
			_targetTransformQuery = GetEntityQuery(
				ComponentType.Exclude<NavMeshAgent>(),
				ComponentType.ReadOnly<TeleportData>(),
				ComponentType.ReadOnly<AbilityTargetTeleport>());

			_targetNavMeshQuery = GetEntityQuery(
				ComponentType.ReadOnly<NavMeshAgent>(),
				ComponentType.ReadOnly<TeleportData>(),
				ComponentType.ReadOnly<AbilityTargetTeleport>());

			_aliveActorQuery = GetEntityQuery(
				ComponentType.ReadOnly<Actor>(),
				ComponentType.Exclude<DeadActorTag>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_targetTransformQuery).ForEach(
				(Entity entity, ref TeleportData teleportData, AbilityTargetTeleport teleportAbility, Transform transform) =>
				{
					EntityManager.RemoveComponent<TeleportData>(entity);
					teleportAbility.SpawnFX(teleportAbility.PreFX);
					var source = teleportAbility;
					Transform targetTransform = null;
					var distance = float.MaxValue;

					Entities.With(_aliveActorQuery).ForEach(
						(Actor actor) =>
						{
							var distanceToActor = Vector3.Distance(actor.transform.position, source.transform.position);
							if (teleportAbility.AbilityTarget.TagFilter.Filter((IActor)actor) && distanceToActor < distance)
							{
								targetTransform = actor.transform;
								distance = distanceToActor;
								teleportAbility.AbilityTarget.Target = actor;
							}
						});

					var direction = targetTransform.position - transform.position;
					var thresholdVector = direction.normalized * teleportData.PositionThreshold;
					var position = targetTransform.position - thresholdVector;
					transform.position = position;
					teleportAbility.SpawnFX(teleportAbility.PostFX);
				});

			Entities.With(_targetNavMeshQuery).ForEach(
				(Entity entity, ref TeleportData data, AbilityTargetTeleport ability, NavMeshAgent agent, Transform transform) =>
				{
					EntityManager.RemoveComponent<TeleportData>(entity);
					var abilityTarget = ability.AbilityTarget;
					var source = ability;
					Transform targetTransform = null;
					var distance = float.MaxValue;

					Entities.With(_aliveActorQuery).ForEach(
						(Actor actor) =>
						{
							var distanceToActor = Vector3.Distance(actor.transform.position, source.transform.position);
							if (abilityTarget.TagFilter.Filter((IActor)actor) && distanceToActor < distance)
							{
								targetTransform = actor.transform;
								distance = distanceToActor;
								ability.AbilityTarget.Target = actor;
							}
						});
					if (targetTransform == null)
					{
						return;
					}
					var targetPosition = targetTransform.position;
					var positions = NavMeshUtils.CalculatePositionsOnCircle(targetPosition, data.PositionThreshold, 16);
					positions.Shuffle();
					if (!agent.TryGetFirstReachable(positions, out var teleportPostion))
					{
						return;
					}

					ability.SpawnFX(ability.PreFX);
					transform.position = teleportPostion;
					ability.SpawnFX(ability.PostFX);
				});
		}
	}
}