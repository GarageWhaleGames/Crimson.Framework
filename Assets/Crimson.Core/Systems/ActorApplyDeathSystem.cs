using Assets.Crimson.Core.Common.ComponentDatas;
using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	public class ActorApplyDeathSystem : ComponentSystem
	{
		private EntityQuery _destructionActorByTimerQuery;
		private EntityQuery _immediateActorDestructionRectTransformQuery;
		private EntityQuery _immediateActorDestructionTransformQuery;
		private EntityQuery _ressurectQuery;
		private EntityQuery _weaponQuery;

		protected override void OnCreate()
		{
			_weaponQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityWeapon>(),
				ComponentType.ReadOnly<DeadActorTag>());

			_destructionActorByTimerQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityActorPlayer>(),
				ComponentType.ReadOnly<DeadActorTag>(),
				ComponentType.Exclude<ImmediateDestructionActorTag>(),
				ComponentType.Exclude<DestructionPendingTag>());

			_ressurectQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityActorPlayer>(),
				ComponentType.ReadOnly<DeadActorTag>(),
				ComponentType.ReadOnly<RessurectTag>());

			_immediateActorDestructionTransformQuery = GetEntityQuery(ComponentType.ReadOnly<ImmediateDestructionActorTag>(),
				ComponentType.ReadOnly<Transform>());

			_immediateActorDestructionRectTransformQuery = GetEntityQuery(ComponentType.ReadOnly<ImmediateDestructionActorTag>(),
				ComponentType.ReadOnly<RectTransform>());
		}

		protected override void OnUpdate()
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			Entities.With(_ressurectQuery).ForEach(
				(Entity entity, AbilityActorPlayer abilityPlayer) =>
				{
					EntityManager.RemoveComponent<OverdamageData>(entity);
					EntityManager.RemoveComponent<OverdamageFXTag>(entity);
					EntityManager.RemoveComponent<RessurectTag>(entity);
					EntityManager.RemoveComponent<DeadActorTag>(entity);
					EntityManager.RemoveComponent<DestructionPendingTag>(entity);
					EntityManager.RemoveComponent<ImmediateDestructionActorTag>(entity);
				});

			Entities.With(_weaponQuery).ForEach(
				(AbilityWeapon weapon) =>
				{
					weapon.DestroyAimEffects();
				});

			Entities.With(_destructionActorByTimerQuery).ForEach(
				(Entity entity, AbilityActorPlayer actorPlayer) =>
				{
					if (dstManager.HasComponent<UserInputData>(entity)) return;

					foreach (var name in actorPlayer.deadActorBehaviour.OnDeathActionsComponentNames)
					{
						var ability = actorPlayer.Actor.Abilities.FirstOrDefault(a =>
							a is IHasComponentName componentName && componentName.ComponentName.Equals(name));

						ability?.Execute();
					}

					if (actorPlayer.deadActorBehaviour.RemoveInput)
					{
						PostUpdateCommands.RemoveComponent<PlayerInputData>(entity);
					}

					if (!actorPlayer.TimerActive) return;

					if (actorPlayer.NeedCleanup)
					{
						actorPlayer.gameObject.DeathPhysics(entity, actorPlayer.deadActorBehaviour);
						actorPlayer.StartDeathTimer();
					}

					dstManager.AddComponent<DestructionPendingTag>(entity);
				}
			);

			Entities.With(_immediateActorDestructionTransformQuery).ForEach(
				(Entity entity, Transform obj) =>
				{
					if (dstManager.HasComponent<TimerComponent>(entity))
					{
						var t = dstManager.GetComponentObject<TimerComponent>(entity);
						t.TimedActions.Add(TimerUtils.CreateAction(t.DestroyWithEntity, 0.1f));
					}
					else
					{
						obj.gameObject.DestroyWithEntity(entity);
					}
				});

			Entities.With(_immediateActorDestructionRectTransformQuery).ForEach(
				(Entity entity, RectTransform obj) =>
				{
					if (dstManager.HasComponent<TimerComponent>(entity))
					{
						var t = dstManager.GetComponentObject<TimerComponent>(entity);
						t.TimedActions.Add(TimerUtils.CreateAction(t.DestroyWithEntity, 0.1f));
					}
					else
					{
						obj.gameObject.DestroyWithEntity(entity);
					}
				});
		}
	}
}