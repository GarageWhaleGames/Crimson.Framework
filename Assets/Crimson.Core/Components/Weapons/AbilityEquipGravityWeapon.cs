using Assets.Crimson.Core.Common.Interfaces;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class AbilityEquipGravityWeapon : MonoBehaviour, IActorAbility, IEquipable
	{
		[ValidateInput(nameof(MustBeGravityWeapon), "Perk MonoBehaviours must derive from GravityWeapon!")]
		public MonoBehaviour TargetWeapon;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Equip(IActor target)
		{
			if (target == null || TargetWeapon == null)
			{
				Debug.LogError("[Equip Weapon] Error copying component to Actor!");
				return;
			}

			var gravityWeapon = TargetWeapon as GravityWeapon;
			var owner = target.Owner;
			var ownerEntity = owner.ActorEntity;
			gravityWeapon.AddComponentData(ref ownerEntity, owner);

			var hotkeyWeapon = target.GameObject.GetComponent<HotkeyWeapon>();
			if (hotkeyWeapon != null)
			{
				hotkeyWeapon.Add(gravityWeapon);
			}
		}

		public void Execute()
		{
		}

		private bool MustBeGravityWeapon(MonoBehaviour item)
		{
			return item == null || item is GravityWeapon;
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			if (TargetWeapon == null)
			{
				var weapon = GetComponent<IWeapon>();
				TargetWeapon = weapon as MonoBehaviour;
			}
		}

#endif
	}
}