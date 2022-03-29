using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Crimson.Core.Common
{
	[Serializable]
	public struct CustomBinding
	{
		public int index;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions;

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		public static bool operator ==(CustomBinding left, CustomBinding right)
		{
			return left.index == right.index;
		}

		public static bool operator !=(CustomBinding left, CustomBinding right)
		{
			return left.index != right.index;
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