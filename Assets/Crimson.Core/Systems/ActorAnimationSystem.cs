using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using System;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	public class ActorAnimationSystem : ComponentSystem
	{
		private EntityQuery _aimingAnimationQuery;
		private EntityQuery _damagedActorsQuery;
		private EntityQuery _deadActorsQuery;
		private EntityQuery _forceActorsQuery;
		private EntityQuery _movementQuery;
		private EntityQuery _projectileQuery;
		private EntityQuery _reloadAnimationsQuery;
		private EntityQuery _strafeActorsQuery;
		private EntityQuery _meleeQuery;
		private EntityQuery _rangeQuery;

		protected override void OnCreate()
		{
			_meleeQuery = GetEntityQuery(
				ComponentType.Exclude<DeadActorTag>(),
				ComponentType.ReadOnly<AnimationMeleeAttackTag>(),
				ComponentType.ReadOnly<Animator>());

			_rangeQuery = GetEntityQuery(
				ComponentType.Exclude<DeadActorTag>(),
				ComponentType.ReadOnly<AnimationRangeAttackTag>(),
				ComponentType.ReadOnly<Animator>());

			_movementQuery = GetEntityQuery(
				ComponentType.ReadOnly<ActorMovementAnimationData>(),
				ComponentType.ReadOnly<Animator>());

			_projectileQuery = GetEntityQuery(
				ComponentType.ReadOnly<ActorProjectileAnimData>(),
				ComponentType.ReadWrite<ActorProjectileThrowAnimTag>(),
				ComponentType.ReadOnly<Animator>());

			_reloadAnimationsQuery = GetEntityQuery(
				ComponentType.ReadOnly<ReloadTag>(),
				ComponentType.ReadWrite<ReloadAnimationData>(),
				ComponentType.ReadOnly<Animator>());

			_deadActorsQuery = GetEntityQuery(
				ComponentType.ReadOnly<DeadActorTag>(),
				ComponentType.ReadWrite<ActorDeathAnimData>(),
				ComponentType.ReadOnly<Animator>());

			_strafeActorsQuery = GetEntityQuery(
				ComponentType.ReadOnly<StrafeActorTag>(),
				ComponentType.ReadWrite<ActorStafeAnimData>(),
				ComponentType.ReadOnly<Animator>());

			_forceActorsQuery = GetEntityQuery(
				ComponentType.ReadOnly<AdditionalForceActorTag>(),
				ComponentType.ReadWrite<ActorForceAnimData>(),
				ComponentType.ReadOnly<Animator>());

			_damagedActorsQuery = GetEntityQuery(
				ComponentType.ReadWrite<DamagedActorTag>(),
				ComponentType.ReadOnly<ActorTakeDamageAnimData>(),
				ComponentType.ReadOnly<Animator>());

			_aimingAnimationQuery = GetEntityQuery(
				ComponentType.ReadOnly<AimingAnimProperties>(),
				ComponentType.ReadOnly<ActorEvaluateAimingAnimData>(),
				ComponentType.ReadOnly<Animator>());
		}

		protected override void OnUpdate()
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			Entities.With(_movementQuery).ForEach(
				(Entity entity, Animator animator, ref ActorMovementAnimationData animation, ref ActorMovementData movement) =>
				{
					if (animator == null)
					{
						Debug.LogError("[MOVEMENT ANIMATION SYSTEM] No Animator found!");
						return;
					}

					var move = movement.MovementCache;
					if (animation.AnimHash == 0 || animation.SpeedFactorHash == 0)
					{
						Debug.LogError("[MOVEMENT ANIMATION SYSTEM] Some hash(es) not found, check your Actor Movement Component Settings!");
						return;
					}
					if (animator.runtimeAnimatorController == null)
					{
						return;
					}
					animator.SetBool(animation.AnimHash, Math.Abs(move.x) > Constants.MIN_MOVEMENT_THRESH || Math.Abs(move.z) > Constants.MIN_MOVEMENT_THRESH);
					animator.SetFloat(animation.SpeedFactorHash,
						animation.SpeedFactorMultiplier * movement.ExternalMultiplier * Math.Max(Math.Abs(move.x), Math.Abs(move.z)));
				});
			Entities.With(_meleeQuery).ForEach(
				(Entity entity, Animator animator, ref AnimationMeleeAttackTag data) =>
				{
					if (animator == null)
					{
						Debug.LogError("[Melee attack] No Animator found!");
						return;
					}

					if (data.NameHash == 0)
					{
						Debug.LogError("[Melee attack] Some hash(es) not found, check your Actor Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetTrigger(data.NameHash);
					}

					dstManager.RemoveComponent<AnimationMeleeAttackTag>(entity);
				});

			Entities.With(_rangeQuery).ForEach(
				(Entity entity, Animator animator, ref AnimationRangeAttackTag data) =>
				{
					if (animator == null)
					{
						Debug.LogError("[Range attack] No Animator found!");
						return;
					}

					if (data.NameHash == 0)
					{
						Debug.LogError("[Range attack] Some hash(es) not found, check your Actor Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetTrigger(data.NameHash);
					}

					dstManager.RemoveComponent<AnimationRangeAttackTag>(entity);
				});

			Entities.With(_projectileQuery).ForEach(
				(Entity entity, Animator animator, ref ActorProjectileAnimData data) =>
				{
					if (animator == null)
					{
						Debug.LogError("[PROJECTILE THROW ANIMATION SYSTEM] No Animator found!");
						return;
					}

					if (data.AnimHash == 0)
					{
						Debug.LogError("[PROJECTILE THROW ANIMATION SYSTEM] Some hash(es) not found, check your Actor Projectile Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetTrigger(data.AnimHash);
					}
					PostUpdateCommands.RemoveComponent<ActorProjectileThrowAnimTag>(entity);
				});

			Entities.With(_deadActorsQuery).ForEach(
				(Entity entity, Animator animator, ref ActorDeathAnimData animation) =>
				{
					if (animator == null)
					{
						Debug.LogError("[DEATH ANIMATION SYSTEM] No Animator found!");

						dstManager.AddComponent<ImmediateDestructionActorTag>(entity);
						return;
					}

					if (animation.AnimHash == 0)
					{
						Debug.LogError("[DEATH ANIMATION SYSTEM] Some hash(es) not found, check your Actor Death Component Settings!");

						dstManager.AddComponent<ImmediateDestructionActorTag>(entity);
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetBool(animation.AnimHash, true);
					}
					dstManager.RemoveComponent<ActorDeathAnimData>(entity);
				});

			Entities.With(_strafeActorsQuery).ForEach(
				(Entity entity, Animator animator, ref ActorStafeAnimData animation) =>
				{
					if (animator == null)
					{
						Debug.LogError("[STRAFE ANIMATION SYSTEM] No Animator found!");
						return;
					}

					if (animation.AnimHash == 0)
					{
						Debug.LogError("[STRAFE ANIMATION SYSTEM] Some hash(es) not found, check your Actor STRAFE Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetBool(animation.AnimHash, true);
					}
					dstManager.RemoveComponent<ActorStafeAnimData>(entity);
				});

			Entities.With(_forceActorsQuery).ForEach(
				(Entity entity, Animator animator, ref ActorForceAnimData animation) =>
				{
					if (animator == null)
					{
						Debug.LogError("[AdditionalForce ANIMATION SYSTEM] No Animator found!");
						return;
					}

					if (animation.AnimHash == 0)
					{
						Debug.LogError("[AdditionalForce ANIMATION SYSTEM] Some hash(es) not found, check your Actor Force Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetBool(animation.AnimHash, true);
					}
					dstManager.RemoveComponent<ActorForceAnimData>(entity);
				});

			Entities.With(_damagedActorsQuery).ForEach(
				(Entity entity, Animator animator, ref ActorTakeDamageAnimData animation, ref DamagedActorTag damagedActorData) =>
				{
					if (animator == null)
					{
						Debug.LogError("[DAMAGE ANIMATION SYSTEM] No Animator found!");
						return;
					}

					if (animation.AnimHash == 0)
					{
						Debug.LogError("[DAMAGE ANIMATION SYSTEM] Some hash(es) not found, check your Actor Damage Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetTrigger(animation.AnimHash);
					}
					dstManager.RemoveComponent<DamagedActorTag>(entity);
				});

			Entities.With(_aimingAnimationQuery).ForEach(
				(Entity entity, Animator animator, ref AimingAnimProperties animation, ref ActorEvaluateAimingAnimData aimingAnimData) =>
				{
					if (animator == null)
					{
						Debug.LogError("[AIMING ANIMATION SYSTEM] No Animator found!");
						return;
					}

					if (animation.AnimHash == 0)
					{
						Debug.LogError("[AIMING ANIMATION SYSTEM] Some hash(es) not found, check your Actor Damage Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetBool(animation.AnimHash, aimingAnimData.AimingActive);
					}
				});

			Entities.With(_reloadAnimationsQuery).ForEach(
				(Entity entity, Animator animator, ref ReloadAnimationData data) =>
				{
					if (animator == null)
					{
						Debug.LogError("[RELOAD ANIMATION SYSTEM] No Animator found!");
						return;
					}

					if (data.AnimHash == 0)
					{
						Debug.LogError("[RELOAD ANIMATION SYSTEM] Some hash(es) not found, check your Actor Weapon Component Settings!");
						return;
					}

					if (animator.runtimeAnimatorController != null)
					{
						animator.SetTrigger(data.AnimHash);
					}
					dstManager.RemoveComponent<ReloadTag>(entity);
				});
		}
	}
}
