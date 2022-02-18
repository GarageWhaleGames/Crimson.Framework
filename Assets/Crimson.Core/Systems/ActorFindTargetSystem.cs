using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Components.AbilityReactive;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Systems
{
	public class ActorFindTargetSystem : ComponentSystem
	{
		private EntityQuery _queryFollowMovement, _queryFollowRotation, _queryAutoAim, _newAutoAimQuery;

		protected override void OnCreate()
		{
			_queryFollowMovement = GetEntityQuery(
				ComponentType.ReadOnly<ActorFollowMovementData>(),
				ComponentType.ReadWrite<ActorNoFollowTargetMovementData>(),
				ComponentType.ReadOnly<AbilityFollowMovement>(),
				ComponentType.Exclude<DeadActorTag>());

			_queryFollowRotation = GetEntityQuery(
				ComponentType.ReadOnly<ActorFollowRotationData>(),
				ComponentType.ReadWrite<ActorNoFollowTargetRotationData>(),
				ComponentType.ReadOnly<AbilityFollowRotation>(),
				ComponentType.Exclude<DeadActorTag>());

			_queryAutoAim = GetEntityQuery(
				ComponentType.ReadOnly<FindAutoAimTargetData>(),
				ComponentType.ReadOnly<Actor>(),
				ComponentType.Exclude<DeadActorTag>());

			_newAutoAimQuery = GetEntityQuery(
				ComponentType.ReadOnly<AutoAimTargetData>(),
				ComponentType.ReadOnly<Actor>(),
				ComponentType.Exclude<DeadActorTag>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_queryFollowMovement).ForEach(
				(Entity entity, AbilityFollowMovement follow, ref ActorFollowMovementData followData) =>
				{
					if (follow.Target == null)
					{
						var properties = follow.findTargetProperties;

						var targets = GetTargetList(follow.Actor, properties.targetType,
							properties.actorWithComponentName, properties.targetTag);

						if (properties.ignoreSpawner && targets.Contains(follow.Actor.Spawner.GameObject.transform))
						{
							targets.Remove(follow.Actor.Spawner.GameObject.transform);
						}

						follow.Target =
							FindActorsUtils.ChooseActor(follow.gameObject.transform, targets, properties.strategy);

						if (follow.Target == null)
						{
							return;
						}
					}

					followData.Origin = follow.Target.position;
					PostUpdateCommands.RemoveComponent<ActorNoFollowTargetMovementData>(entity);
				}
			);

			Entities.With(_queryFollowRotation).ForEach(
				(Entity entity, AbilityFollowRotation follow, ref ActorFollowRotationData followData) =>
				{
					if (follow.target == null)
					{
						var targets = GetTargetList(follow.Actor, follow.followTarget,
							follow.actorWithComponentName, follow.targetTag);

						follow.target =
							FindActorsUtils.ChooseActor(follow.gameObject.transform, targets, follow.strategy);

						if (follow.target == null)
						{
							return;
						}
					}

					followData.Origin = follow.target.rotation.eulerAngles;
					PostUpdateCommands.RemoveComponent<ActorNoFollowTargetRotationData>(entity);
					var t = typeof(ActorNoFollowTargetRotationData);
					PostUpdateCommands.RemoveComponent(entity, t);
				}
			);

			Entities.With(_queryAutoAim).ForEach(
				(Entity entity, Actor actor, ref FindAutoAimTargetData findAutoAimTargetData) =>
				{
					var autoAimData = findAutoAimTargetData;
					var weapon = actor.Abilities.OfType<AbilityWeapon>()
						.FirstOrDefault(a => a.componentName == autoAimData.WeaponComponentName);

					if (weapon == null)
					{
						PostUpdateCommands.RemoveComponent<FindAutoAimTargetData>(entity);
						return;
					}

					var properties = weapon.findTargetProperties;

					var targets = GetTargetList(weapon.Actor, properties.targetType,
						properties.actorWithComponentName, properties.targetTag);

					if (properties.ignoreSpawner && targets.Contains(weapon.Actor.GameObject.transform))
					{
						targets.Remove(weapon.Actor.GameObject.transform);
					}

					var targetTransform =
						FindActorsUtils.ChooseActor(weapon.gameObject.transform, targets, properties.strategy);

					if (targetTransform == null || (properties.strategy == ChooseTargetStrategy.Nearest
						&& properties.maxDistanceThreshold > 0f
						&& math.distancesq(targetTransform.position, weapon.Actor.GameObject.transform.position) >
						properties.maxDistanceThreshold * properties.maxDistanceThreshold))
					{
						properties.SearchCompleted = true;
						PostUpdateCommands.RemoveComponent<FindAutoAimTargetData>(entity);

						weapon.Spawn();
						return;
					}

					weapon.DisposableSpawnCallback = go =>
					{
						var targetActor = go.GetComponent<Actor>();
						if (targetActor == null)
						{
							return;
						}

						targetActor.ChangeActorForceMovementData(go.transform.forward);
						weapon.DisposableSpawnCallback = null;
					};
					if (!weapon.aimingByInput)
					{
						actor.GameObject.transform.LookAt(targetTransform.position);
					}
					//weapon.SpawnPointsRoot.LookAt(targetTransform.position);
					properties.SearchCompleted = true;
					weapon.Spawn();
					PostUpdateCommands.RemoveComponent<FindAutoAimTargetData>(entity);
				}
			);

			Entities.With(_newAutoAimQuery).ForEach(
				(Entity entity, Actor actor, ref AutoAimTargetData findAutoAimTargetData) =>
				{
					var autoAimData = findAutoAimTargetData;
					var autoAim = actor.GetComponent<AutoAim>();
					if (autoAim == null)
					{
						PostUpdateCommands.RemoveComponent<AutoAimTargetData>(entity);
						return;
					}
					var properties = autoAim.FindTargetProperties;

					var targets = GetTargetList(actor, properties.targetType,
						properties.actorWithComponentName, properties.targetTag);

					if (properties.ignoreSpawner && targets.Contains(actor.GameObject.transform))
					{
						targets.Remove(actor.GameObject.transform);
					}

					var targetTransform =
						FindActorsUtils.ChooseActor(actor.gameObject.transform, targets, properties.strategy);

					if (targetTransform == null || (properties.strategy == ChooseTargetStrategy.Nearest
						&& properties.maxDistanceThreshold > 0f
						&& math.distancesq(targetTransform.position, actor.GameObject.transform.position) <
						properties.maxDistanceThreshold * properties.maxDistanceThreshold))
					{
						properties.SearchCompleted = true;
						autoAim.ResetAiming();
						PostUpdateCommands.RemoveComponent<AutoAimTargetData>(entity);

						return;
					}

					autoAim.SetTarget(targetTransform.position);
					properties.SearchCompleted = true;
					PostUpdateCommands.RemoveComponent<AutoAimTargetData>(entity);
				}
			);
		}

		private List<Transform> GetTargetList(IActor source, TargetType followTarget, string name, string tag)
		{
			var targets = new List<Transform>();

			switch (followTarget)
			{
				case TargetType.ComponentName:
					Entities.WithAll<ActorData>().WithNone<DeadActorTag, DestructionPendingTag>().ForEach(
						(Entity entity, Transform obj) => targets.AddRange(from component in obj.gameObject.GetComponents<IComponentName>()
																		   where component.ComponentName.Equals(name, StringComparison.Ordinal)
																		   select obj));
					break;

				case TargetType.ChooseByTag:
					Entities.WithAll<ActorData>().WithNone<DeadActorTag, DestructionPendingTag>().ForEach(
						(Entity entity, Transform obj) =>
						{
							if (obj.CompareTag(tag))
							{
								targets.Add(obj);
							}
						}
					);
					if (targets.Count == 0)
					{
						targets = GameObject.FindGameObjectsWithTag(tag).ToList().ConvertAll(g => g.transform);
					}

					break;

				case TargetType.Spawner:
					var t = source?.Spawner;
					if (t != null && t.GameObject != null)
					{
						targets.Add(t.GameObject.transform);
					}

					break;

				case TargetType.None:
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			return targets;
		}
	}
}