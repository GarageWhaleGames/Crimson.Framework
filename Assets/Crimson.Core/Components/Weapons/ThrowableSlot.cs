using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class ThrowableSlot : MonoBehaviour, IActorAbility
	{
		public InputActionReference _executeAction;

		[CastToUI("CurrentThrowable")]
		public IThrowable _weapon;

		[ValidateInput(nameof(MustBeThrowable), "MonoBehaviours must derive from IThrowable!")]
		public MonoBehaviour ThrowableWeapon;

		[HideInInspector] public UIReceiverList UIReceiverList = new UIReceiverList();
		public IActor Actor { get; set; }
		public bool IsEmpty => _weapon == null;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			UIReceiverList.Init(this, entity);
			if (_executeAction != null)
			{
				_executeAction.action.performed += ThrowActionHandler;
			}

			if (ThrowableWeapon != null)
			{
				Change((IThrowable)ThrowableWeapon);
			}
		}

		public void Change(IThrowable weapon)
		{
			_weapon = weapon;
			UIReceiverList.UpdateUIData("CurrentThrowable");
		}

		public void Execute()
		{
			_weapon?.Execute();
		}

		private bool MustBeThrowable(MonoBehaviour item)
		{
			return item == null || item is IThrowable;
		}

		private void OnDestroy()
		{
			if (_executeAction != null)
			{
				_executeAction.action.performed -= ThrowActionHandler;
			}
		}

		private void ThrowActionHandler(InputAction.CallbackContext obj)
		{
			Execute();
		}
	}
}