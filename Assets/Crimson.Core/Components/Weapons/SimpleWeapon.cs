using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Types;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class SimpleWeapon :
		MonoBehaviour,
		IWeapon
	{
		public WeaponType _weaponType;
		public WeaponType Type => _weaponType;

		public Entity _entity;

		public event System.Action OnShot;

		public IActor Actor { get; set; }

		public bool IsEnable { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
		}

		public void Execute()
		{
		}

		public void Reload()
		{
		}

		public void StartFire()
		{
			OnShot?.Invoke();
		}

		public void StopFire()
		{
		}
	}
}