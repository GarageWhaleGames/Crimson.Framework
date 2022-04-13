using Crimson.Core.Components;
using Crimson.Core.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[Serializable]
	public class RandomActionBehaviour : IAIBehaviour
	{
		public string XAxis => "";
		public string[] AdditionalModes => new string[0];
		public bool NeedCurve => false;
		public bool NeedTarget => false;
		public bool NeedActions => true;

		private const float ACTION_DELAY = 6f;

		private AIBehaviourSetting _behaviour = null;
		private List<IActorAbility> _abilities = new List<IActorAbility>();
		private AbilityPlayerInput _input;

		private AbilityPlayerInput CustomInput
		{
			get
			{
				if (_input == null)
				{
					_input = _behaviour.Actor.GameObject.GetComponent<AbilityPlayerInput>();
				}

				return _input;
			}
		}

		public bool HasDistanceLimit => false;

		public float Evaluate(Entity entity, AIBehaviourSetting behaviour, AbilityAIInput ai, List<Transform> targets)
		{
			if (this.GetType().ToString().Contains(ai.activeBehaviour.behaviourType))
			{
				return 0f;
			}

			_behaviour = behaviour;

			if (Time.timeSinceLevelLoad < ACTION_DELAY * Random.value) return 0f;

			if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity)) return 0f;

			if (CustomInput == null)
			{
				//Debug.Log("[ATTACK BEHAVIOUR] No Input Ability!");
				return 0f;
			}

			if (CustomInput.bindingsDict.ContainsKey(_behaviour.executeCustomInput))
			{
				_abilities = CustomInput.bindingsDict[_behaviour.executeCustomInput];
			}
			else
			{
				//Debug.Log($"[ATTACK BEHAVIOUR] Custom Input {_behaviour.executeCustomInput} in Attack Behaviour not set in Input Ability!");
				return 0f;
			}

			if (!_abilities.ActionPossible())
			{
				//Debug.Log($"[ATTACK BEHAVIOUR] Custom Input {_behaviour.executeCustomInput} has no actions!");
				return 0f;
			}

			return behaviour.basePriority * Random.value;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			return true;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (!_abilities.ActionPossible()) return false;

			inputData.CustomInput[_behaviour.executeCustomInput] = 1f;

			return true;
		}
	}
}