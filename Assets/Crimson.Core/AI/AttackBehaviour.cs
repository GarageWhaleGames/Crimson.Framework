using Crimson.Core.Components;
using Crimson.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[Serializable]
	public class AttackBehaviour : IAIBehaviour
	{
		public string XAxis => "distance to closest seen target";
		public string[] AdditionalModes => new string[0];
		public bool NeedCurve => true;
		public bool NeedTarget => true;
		public bool NeedActions => true;

		private const float AIM_MAX_DIST = 40f;
		private const float ATTACK_DELAY = 6f;
		private readonly Vector3 VIEW_POINT_DELTA = new Vector3(0f, 0.6f, 0f);

		private Transform _target = null;
		private Transform _transform = null;
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

			_target = null;
			_behaviour = behaviour;
			_transform = behaviour.Actor.GameObject.transform;

			if (Time.timeSinceLevelLoad < ATTACK_DELAY * Random.value
				|| World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity)
				|| CustomInput == null)
			{
				return 0f;
			}

			if (CustomInput.bindingsDict.ContainsKey(_behaviour.executeCustomInput))
			{
				_abilities = CustomInput.bindingsDict[_behaviour.executeCustomInput];
			}
			else
			{
				return 0f;
			}

			if (!_abilities.ActionPossible())
			{
				return 0f;
			}

			var orderedTargets = targets
				.Where(t => t.FilterTag(_behaviour) && t != _transform)
				.OrderBy(t => math.distancesq(_transform.position, t.position));

			foreach (var t in orderedTargets)
			{
				if (Physics.Raycast(_transform.position + VIEW_POINT_DELTA, t.position - _transform.position, out var hit,
					AIM_MAX_DIST))
				{
					if (hit.transform != t)
					{
						continue;
					}

					_target = t;

					var dist = math.distance(_transform.position, _target.position);
					var sampleScale = _behaviour.curveMaxSample - _behaviour.curveMinSample;
					var curveSample = math.clamp(
						(dist - _behaviour.curveMinSample) / sampleScale, 0f, 1f);
					var result = _behaviour.priorityCurve.Evaluate(curveSample) * _behaviour.basePriority;

					return result;
				}
			}

			return 0f;
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

			if (Physics.Raycast(_transform.position + VIEW_POINT_DELTA, _target.position - _transform.position, out var hit,
				AIM_MAX_DIST) && hit.transform == _target)
			{
				inputData.CustomInput[_behaviour.executeCustomInput] = 1f;
				return true;
			}

			return false;
		}
	}
}