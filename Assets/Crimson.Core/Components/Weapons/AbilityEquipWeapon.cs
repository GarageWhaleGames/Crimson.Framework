using Assets.Crimson.Core.Common.Interfaces;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class AbilityEquipWeapon : MonoBehaviour, IActorAbility, IEquipable
	{
		[ValidateInput(nameof(MustBeWeapon), "Perk MonoBehaviours must derive from IWeapon!")]
		public MonoBehaviour TargetWeapon;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Equip(IActor target)
		{
			if (TargetWeapon == null)
			{
				return;
			}

			var weapon = TargetWeapon as IWeapon;

			var hotkeyWeapon = target.GameObject.GetComponent<HotkeyWeapon>();
			if (hotkeyWeapon != null)
			{
				hotkeyWeapon.Add(weapon);
			}
		}

		public void Execute()
		{
		}

		private bool MustBeWeapon(MonoBehaviour item)
		{
			return item == null || item is IWeapon;
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