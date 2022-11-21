using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components
{
	public class AbilityButtonOnceInputRef : MonoBehaviour, IActorAbility
	{
		public List<IActorAbility> _actions;
		public InputActionReference _input;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions;

		public IActor Actor { get; set; }
		public Entity Entity { get; private set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Entity = entity;
			Actor = actor;

			if (_input != null)
			{
				_input.action.performed += Click;
			}

			_actions = actions.ConvertAll(s => s as IActorAbility);
		}

		public void Execute()
		{
		}

		private void Click(InputAction.CallbackContext context)
		{
			for (var i = 0; i < _actions.Count; i++)
			{
				_actions[i]?.Execute();
			}
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		private void OnDestroy()
		{
			if (_input != null)
			{
				_input.action.performed -= Click;
			}
		}
	}
}