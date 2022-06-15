using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
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
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_bindings.MoveInputRef.action.performed += ReadMove;
			_bindings.MoveInputRef.action.canceled += ReadMove;

			_bindings.LookInputRef.action.performed += ReadLook;
			_bindings.LookInputRef.action.canceled += ReadLook;

			SubscribeCustomInput(0, _bindings.CustomInput0Ref);
			SubscribeCustomInput(1, _bindings.CustomInput1Ref);
			SubscribeCustomInput(2, _bindings.CustomInput2Ref);
			SubscribeCustomInput(3, _bindings.CustomInput3Ref);
			SubscribeCustomInput(4, _bindings.CustomInput4Ref);
			SubscribeCustomInput(5, _bindings.CustomInput5Ref);
		}

		public void Execute()
		{
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
			inputData.Mouse = context.ReadValue<Vector2>();
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

		private void SubscribeCustomInput(int index, InputAction action)
		{
			action.performed += (context) => ReadCustomInput(index, context);
			action.canceled += (context) => ReadCustomInput(index, context);
		}
	}
}