﻿using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class HotkeyWeapon : MonoBehaviour, IActorAbility
	{
		public InputActionReference _activateGravigunAction;
		public InputActionReference _changeAction;
		public InputActionReference _changeThrowableAction;
		public WeaponSlot _slot;
		public ThrowableSlot _throwableSlot;

		[ValidateInput(nameof(MustBeWeapon), "Perk MonoBehaviours must derive from IWeapon!")]
		public List<MonoBehaviour> Weapons;

		private readonly List<IThrowable> _throwables = new List<IThrowable>();
		private readonly List<IWeapon> _weapons = new List<IWeapon>();
		private IWeapon _gravityGun;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_weapons.AddRange(Weapons.Cast<IWeapon>());
			if (_changeAction != null)
			{
				_changeAction.action.performed += ChangeActionHandler;
			}
			if (_changeThrowableAction != null)
			{
				_changeThrowableAction.action.performed += ChangeThrowableActionHandler;
			}
			if (_activateGravigunAction != null)
			{
				_activateGravigunAction.action.performed += ToggleGravigunHandler;
			}
			_slot.IsEnable = true;

			if (!actor.Abilities.Contains(this))
			{
				actor.Abilities.Add(this);
			}
		}

		public void Execute()
		{
		}

		internal void Add(IWeapon weapon)
		{
			if (weapon is GravityWeapon)
			{
				_gravityGun = weapon;
				_slot.Change(weapon);
			}
			else
			{
				_weapons.Add(weapon);
				_slot.Change(weapon);
			}
		}

		internal void Add(IAmmo ammo)
		{
			for (var i = 0; i < _weapons.Count; i++)
			{
				if (_weapons[i].ComponentName == ammo.Target.ComponentName)
				{
					_weapons[i].AddAmmo(ammo);
					return;
				}
			}

			for (var i = 0; i < _throwables.Count; i++)
			{
				if (_throwables[i].ComponentName == ammo.Target.ComponentName)
				{
					_throwables[i].AddAmmo(ammo);
					return;
				}
			}
		}

		internal void Add(IThrowable throwable)
		{
			_throwables.Add(throwable);
			_throwableSlot.Change(throwable);
		}

		private void ChangeActionHandler(InputAction.CallbackContext obj)
		{
			SelectNextWeapon();
		}

		private void ChangeThrowableActionHandler(InputAction.CallbackContext obj)
		{
			SelectNextThrowable();
		}

		private bool MustBeWeapon(List<MonoBehaviour> actions)
		{
			foreach (var action in actions)
			{
				if (action is IWeapon || action is null)
				{
					continue;
				}

				return false;
			}

			return true;
		}

		private void OnDestroy()
		{
			if (_changeAction != null)
			{
				_changeAction.action.performed -= ChangeActionHandler;
			}
			if (_changeThrowableAction != null)
			{
				_changeThrowableAction.action.performed -= ChangeThrowableActionHandler;
			}
			if (_activateGravigunAction != null)
			{
				_activateGravigunAction.action.performed -= ToggleGravigunHandler;
			}
		}

		private void OnValidate()
		{
			if (_throwableSlot == null)
			{
				_throwableSlot = GetComponentInChildren<ThrowableSlot>();
			}
		}

		private void SelectNextThrowable()
		{
			var currentIndex = _throwables.IndexOf(_throwableSlot._weapon);
			var index = currentIndex == -1 ? 0 : (currentIndex + 1) % _throwables.Count;
			if (_throwables.Count > index)
			{
				_throwableSlot.Change(_throwables[index]);
			}
		}

		private void SelectNextWeapon()
		{
			var currentIndex = _weapons.IndexOf(_slot._weapon);
			var index = currentIndex == -1 ? 0 : (currentIndex + 1) % _weapons.Count;
			if (_weapons.Count > index)
			{
				_slot.Change(_weapons[index]);
			}
		}

		private void ToggleGravigunHandler(InputAction.CallbackContext obj)
		{
			if (_gravityGun == null)
			{
				return;
			}

			_slot.Change(_gravityGun);
		}
	}
}