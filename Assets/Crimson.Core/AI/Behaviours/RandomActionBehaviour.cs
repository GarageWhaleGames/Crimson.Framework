using Assets.Crimson.Core.AI.GeneralParams;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[Serializable, HideMonoScript]
	public class RandomActionBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public ExecutableCustomInput ExecutableInput;

		private const float ACTION_DELAY = 6f;

		private List<IActorAbility> _abilities = new List<IActorAbility>();
		private AbilityPlayerInput _input;

		private AbilityPlayerInput CustomInput
		{
			get
			{
				if (_input == null)
				{
					_input = Actor.GameObject.GetComponent<AbilityPlayerInput>();
				}

				return _input;
			}
		}

		public IActor Actor { get; set; }

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			return Time.timeSinceLevelLoad < ACTION_DELAY * Random.value
				|| World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity)
				|| CustomInput == null
				|| !CustomInput.bindingsDict.TryGetValue(ExecutableInput.CustomInputIndex, out _abilities)
				|| !_abilities.ActionPossible()
				? 0f
				: Priority.Value * Random.value;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			return true;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (!_abilities.ActionPossible())
			{
				return false;
			}

			inputData.CustomInput[ExecutableInput.CustomInputIndex] = 1f;

			return true;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
		}
	}
}