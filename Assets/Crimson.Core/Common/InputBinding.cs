using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Common
{
	[Serializable]
	public struct InputBinding
	{
		[SerializeField] private InputActionReference _inputAction;
		public Guid ID => _inputAction.action.id;

		public InputAction Input => _inputAction.action;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions;

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		public static bool operator ==(InputBinding left, InputBinding right)
		{
			return left.ID == right.ID;
		}

		public static bool operator !=(InputBinding left, InputBinding right)
		{
			return left.ID != right.ID;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
	}
}