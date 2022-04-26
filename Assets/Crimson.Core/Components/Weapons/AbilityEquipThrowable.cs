using Assets.Crimson.Core.Common.Interfaces;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class AbilityEquipThrowable : MonoBehaviour, IActorAbility, IEquipable
	{
		[ValidateInput(nameof(Validate), "Perk MonoBehaviours must derive from IThrowable!")]
		public MonoBehaviour TargetWeapon;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			if (actor.GameObject.GetComponent<HotkeyWeapon>())
			{
				Equip(actor);
			}
		}

		public void Equip(IActor target)
		{
			if (!(target.GameObject.CopyComponent(TargetWeapon) is IThrowable copy))
			{
				Debug.LogError("[Equip Weapon] Error copying component to Actor!");
				return;
			}

			var e = target.ActorEntity;
			copy.AddComponentData(ref e, target);

			var hotkey = target.GameObject.GetComponent<HotkeyWeapon>();
			if (hotkey != null)
			{
				hotkey.Add(copy);
			}
		}

		public void Execute()
		{
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			if (TargetWeapon == null)
			{
				var weapon = GetComponent<IThrowable>();
				TargetWeapon = weapon as MonoBehaviour;
			}
		}

#endif

		private bool Validate(MonoBehaviour item)
		{
			return item == null || item is IThrowable;
		}
	}
}