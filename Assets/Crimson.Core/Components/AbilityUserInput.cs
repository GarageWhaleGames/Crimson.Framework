using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components
{
	public class AbilityUserInput : MonoBehaviour, IActorAbility
	{
		public UserInputBinding _bindings;
		public Entity _entity;

		public EntityManager _entityManager;
		private Dictionary<int, Action<InputAction.CallbackContext>> _customInputCallbacks = new Dictionary<int, Action<InputAction.CallbackContext>>();
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			SubscribeInput(_bindings.MoveInputRef, ReadMove);
			SubscribeInput(_bindings.LookInputRef, ReadLook);
			SubscribeInput(_bindings.PointerInputRef, ReadPointer);

			for (var i = 0; i < _bindings.CustomInputs.Count; i++)
			{
				_customInputCallbacks.Add(i, (context) => ReadCustomInput(i, context));
				SubscribeInput(_bindings.CustomInputs[i], _customInputCallbacks[i]);
			}
		}

		public void Execute()
		{
		}

		private void OnDestroy()
		{
			UnsubscribeInput(_bindings.MoveInputRef, ReadMove);
			UnsubscribeInput(_bindings.LookInputRef, ReadLook);
			UnsubscribeInput(_bindings.PointerInputRef, ReadPointer);

			for (var i = 0; i < _bindings.CustomInputs.Count; i++)
			{
				UnsubscribeInput(_bindings.CustomInputs[i], _customInputCallbacks[i]);
			}
		}

		private void ReadCustomInput(int index, InputAction.CallbackContext context)
		{
			if (_entityManager.HasComponent<DeadActorTag>(_entity))
			{
				return;
			}

			var inputData = _entityManager.GetComponentData<PlayerInputData>(_entity);

			inputData.CustomInput[index] = context.ReadValue<float>();
			_entityManager.SetComponentData(_entity, inputData);
		}

		private void ReadLook(InputAction.CallbackContext context)
		{
			if (_entityManager.HasComponent<DeadActorTag>(_entity))
			{
				return;
			}

			var inputData = _entityManager.GetComponentData<PlayerInputData>(_entity);
			inputData.Look = context.ReadValue<Vector2>();
			_entityManager.SetComponentData(_entity, inputData);
		}

		private void ReadMove(InputAction.CallbackContext context)
		{
			if (_entityManager.HasComponent<DeadActorTag>(_entity))
			{
				return;
			}

			var inputData = _entityManager.GetComponentData<PlayerInputData>(_entity);
			inputData.Move = context.ReadValue<Vector2>();
			_entityManager.SetComponentData(_entity, inputData);
		}

		private void ReadPointer(InputAction.CallbackContext context)
		{
			if (_entityManager.HasComponent<DeadActorTag>(_entity))
			{
				return;
			}

			var inputData = _entityManager.GetComponentData<PlayerInputData>(_entity);
			inputData.Mouse = context.ReadValue<Vector2>();
			_entityManager.SetComponentData(_entity, inputData);
		}

		private void SubscribeInput(InputAction action, Action<InputAction.CallbackContext> callback)
		{
			action.performed += callback;
			action.canceled += callback;
		}

		private void UnsubscribeInput(InputAction action, Action<InputAction.CallbackContext> callback)
		{
			action.performed -= callback;
			action.canceled -= callback;
		}
	}
}