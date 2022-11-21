using Assets.Crimson.Core.Common.Weapons;
using Assets.Crimson.Core.Components.Tags.Weapons;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class WeaponSlot : TimerBaseBehaviour, IActorAbility
	{
		public InputActionReference _executeAction;
		public InputActionReference _reloadAction;
		public float _weaponChangeDuration = 1f;

		[ValidateInput(nameof(MustBeWeapon), "Perk MonoBehaviours must derive from IWeapon!")]
		public MonoBehaviour Weapon;

		[HideInInspector] public UIReceiverList UIReceiverList = new UIReceiverList();

		[CastToUI("CurrentWeapon")]
		public IWeapon _weapon;

		private Entity _entity;
		private EntityManager _entityManager;

		public IActor Actor { get; set; }

		public bool IsEnable { get; set; }
		public bool EnableOnStart;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			Actor = actor;
			UIReceiverList.Init(this, entity);
			if (_executeAction != null)
			{
				_executeAction.action.performed += ExectionActionPerformed;
				_executeAction.action.canceled += ExectionActionCanceled;
			}
			if (_reloadAction != null)
			{
				_reloadAction.action.performed += ReloadActionHandler;
			}

			_entityManager.AddComponentData(_entity, new EquipedWeaponData());

			Timer.Start();

			if (EnableOnStart)
				IsEnable = true;

			if (Weapon != null)
			{
				Change((IWeapon)Weapon);
			}
		}

		public void Change(IWeapon weapon)
		{
			if (!IsEnable)
			{
				return;
			}
			var weaponData = new EquipedWeaponData();
			if (_weapon != null)
			{
				_weapon.IsEnable = false;
				weaponData.Previous = (byte)_weapon.Type;
			}
			_weapon = weapon;
			_weapon.IsEnable = true;
			weaponData.Current = (byte)weapon.Type;
			_entityManager.AddComponentData(_entity, weaponData);
			_entityManager.AddComponentData(_entity, new StartChangeWeaponAnimTag());
			Timer.TimedActions.AddAction(EndChangeWeapon, _weaponChangeDuration);

			UIReceiverList.UpdateUIData("CurrentWeapon");
		}

		private void EndChangeWeapon()
		{
			_entityManager.AddComponentData(_entity, new EndChangeWeaponAnimTag());
		}

		public void Execute()
		{
			if (!IsEnable)
			{
				return;
			}
			_weapon?.StartFire();
		}

		private void ExectionActionCanceled(InputAction.CallbackContext obj)
		{
			if (!IsEnable)
			{
				return;
			}
			_weapon?.StopFire();
		}

		private void ExectionActionPerformed(InputAction.CallbackContext obj)
		{
			if (!IsEnable)
			{
				return;
			}
			Execute();
		}

		private bool MustBeWeapon(MonoBehaviour item)
		{
			return item == null || item is IWeapon;
		}

		private void ReloadActionHandler(InputAction.CallbackContext obj)
		{
			if (!IsEnable)
			{
				return;
			}
			_weapon?.Reload();
		}
	}
}