using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components
{
	public class AbilityButtonInputRef : MonoBehaviour, IActorAbility
	{
		public List<IActorAbility> _actions;
		public InputActionReference _input;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions;

		private EntityManager _dstManager;
		public IActor Actor { get; set; }
		public Entity Entity { get; private set; }
		public bool IsActivated { get; private set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Entity = entity;
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (_input != null)
			{
				_input.action.performed += StartAction;
				_input.action.canceled += StopAction;
				_input.action.Enable();
			}

			_actions = actions.ConvertAll(s => s as IActorAbility);
		}

		public void Execute()
		{
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		private void OnDestroy()
		{
			if (_input == null)
			{
				return;
			}
			_input.action.performed -= StartAction;
			_input.action.canceled -= StopAction;
		}

		private void StartAction(InputAction.CallbackContext context)
		{
			IsActivated = true;
			_dstManager.AddComponentData(Entity, new InputExecuteTag());
		}

		private void StopAction(InputAction.CallbackContext context)
		{
			_dstManager.RemoveComponent<InputExecuteTag>(Entity);
			IsActivated = false;
		}
	}
}