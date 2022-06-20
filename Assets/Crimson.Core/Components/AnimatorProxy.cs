﻿using Assets.Crimson.Core.Common.AnimatorProperties;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	[HideMonoScript]
	public class AnimatorProxy : MonoBehaviour, IActorAbility
	{
		[Title(nameof(RealSpeed))]
		public AnimatorFloat2 RealSpeed;

		[Title(nameof(LookAtDirection))] public AnimatorNormalizedFloat2 LookAtDirection;

		[Title(nameof(CurrentWeapon))] public AnimatorTypeValue CurrentWeapon;
		[Title(nameof(PreviousWeapon))] public AnimatorTypeValue PreviousWeapon;

		[Title(nameof(WeaponChange))] public AnimatorTrigger WeaponChange;
		[Title(nameof(Crouch))] public AnimatorBool Crouch;
		[Title(nameof(Attacking))] public AnimatorBool Attacking;

		[Title(nameof(AttackType))] public AnimatorTypeValue AttackType;

		[Title(nameof(Hit))] public AnimatorBool Hit;
		[Title(nameof(HitDirection))] public AnimatorFloat2 HitDirection;

		[Title(nameof(Reloading))] public AnimatorBool Reloading;
		[Title(nameof(Dodge))] public AnimatorBool Dodge;

		[Title(nameof(Falling))] public AnimatorBool Falling;

		[Title(nameof(Interact))] public AnimatorBool Interact;
		[Title(nameof(InteractType))] public AnimatorTypeValue InteractType;

		[Title(nameof(Death))] public AnimatorTrigger Death;
		[Title(nameof(IsDead))] public AnimatorBool IsDead;

		[Title(nameof(KnockbackStart))] public AnimatorTrigger KnockbackStart;
		[Title(nameof(KnockbackFly))] public AnimatorBool KnockbackFly;
		[Title(nameof(KnockbackGround))] public AnimatorBool KnockbackGround;
		[Title(nameof(KnockbackStandUp))] public AnimatorTrigger KnockbackStandUp;

		[Title(nameof(IdleFun))] public AnimatorTrigger IdleFun;
		[Title(nameof(IdleFundID))] public AnimatorTypeValue IdleFundID;

		[Title(nameof(ItemUse))] public AnimatorTrigger ItemUse;
		[Title(nameof(ItemUseID))] public AnimatorTypeValue ItemUseID;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
		}

		private void Reset()
		{
			RealSpeed = new AnimatorFloat2()
			{
				NameX = "fRealSpeedForward",
				NameY = "fRealSpeedTangent",
				NameLength = "fRealSpeedMagnitude"
			};

			LookAtDirection = new AnimatorNormalizedFloat2()
			{
				NameX = "fLookAtDirectionForward",
				NameY = "fLookAtDirectionTangent"
			};

			CurrentWeapon = new AnimatorTypeValue()
			{
				FloatName = "fCurrentWeaponCategory",
				IntName = "CurrentWeaponCategory"
			};

			PreviousWeapon = new AnimatorTypeValue()
			{
				FloatName = "fPreviousWeaponCategory",
				IntName = "PreviousWeaponCategory"
			};

			WeaponChange = new AnimatorTrigger()
			{
				Name = "tWeaponChangeStart"
			};

			Crouch = new AnimatorBool()
			{
				Name = "isCrouch"
			};

			Attacking = new AnimatorBool()
			{
				Name = "isAttacking"
			};

			AttackType = new AnimatorTypeValue()
			{
				FloatName = "fAttackType",
				IntName = "AttackType"
			};

			Hit = new AnimatorBool()
			{
				Name = "isHit"
			};

			HitDirection = new AnimatorFloat2()
			{
				NameLength = "fHitStrength",
				NameX = "fHitDirectionForward",
				NameY = "fHitDirectionTangent"
			};

			Reloading = new AnimatorBool()
			{
				Name = "isReloading"
			};

			Dodge = new AnimatorBool()
			{
				Name = "isDodge"
			};

			Falling = new AnimatorBool()
			{
				Name = "isFalling"
			};

			Interact = new AnimatorBool()
			{
				Name = "isInteract"
			};

			InteractType = new AnimatorTypeValue()
			{
				FloatName = "fInteractionType",
				IntName = "InteractionType"
			};

			Death = new AnimatorTrigger()
			{
				Name = "tDeath"
			};

			IsDead = new AnimatorBool()
			{
				Name = "isDead"
			};

			KnockbackStart = new AnimatorTrigger()
			{
				Name = "tKnockbackStart"
			};

			KnockbackFly = new AnimatorBool()
			{
				Name = "isKnockbackFly"
			};

			KnockbackGround = new AnimatorBool()
			{
				Name = "isKnockbackGround"
			};

			KnockbackStandUp = new AnimatorTrigger()
			{
				Name = "tKnockbackStandUp"
			};

			IdleFun = new AnimatorTrigger()
			{
				Name = "tIdleFun"
			};

			IdleFundID = new AnimatorTypeValue()
			{
				FloatName = "fIdleFunId",
				IntName = "IdleFunID"
			};

			ItemUse = new AnimatorTrigger()
			{
				Name = "tItemUse"
			};

			ItemUseID = new AnimatorTypeValue()
			{
				FloatName = "fItemID",
				IntName = "itemID"
			};
		}
	}
}