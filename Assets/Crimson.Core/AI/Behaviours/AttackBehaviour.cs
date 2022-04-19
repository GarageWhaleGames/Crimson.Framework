using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[Serializable, HideMonoScript]
	public class AttackBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public CurvePriority CurvePriority = new CurvePriority(0)
		{
			XAxisTooltip = "distance to closest seen target"
		};

		public ExecutableCustomInput ExecutableCustomInput;
		public TagFilter TagFilter;

		private const float AIM_MAX_DIST = 40f;
		private const float ATTACK_DELAY = 6f;
		private readonly Vector3 VIEW_POINT_DELTA = new Vector3(0f, 0.6f, 0f);

		private Transform _target = null;
		private Transform _transform = null;
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

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
		}

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_transform = Actor.GameObject.transform;

			if (Time.timeSinceLevelLoad < ATTACK_DELAY * Random.value
				|| World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity)
				|| CustomInput == null
				|| !CustomInput.bindingsDict.TryGetValue(ExecutableCustomInput.CustomInputIndex, out _abilities)
				|| !_abilities.ActionPossible())
			{
				return 0f;
			}

			var orderedTargets = targets
				.Where(t => TagFilter.Filter(t) && t != _transform)
				.OrderBy(t => math.distancesq(_transform.position, t.position));

			foreach (var t in orderedTargets)
			{
				if (Physics.Raycast(GetDirectionRay(t), out var hit,
					AIM_MAX_DIST))
				{
					if (hit.transform != t)
					{
						continue;
					}

					_target = t;

					var distance = math.distance(_transform.position, _target.position);
					var result = CurvePriority.Evaluate(distance) * Priority.Value;

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

			if (Physics.Raycast(GetDirectionRay(_target), out var hit, AIM_MAX_DIST) && hit.transform == _target)
			{
				inputData.CustomInput[ExecutableCustomInput.CustomInputIndex] = 1f;
				return true;
			}

			return false;
		}

		public void DrawGizmos()
		{
			if (_target == null)
			{
				return;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawRay(GetDirectionRay(_target));
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_target.position, 1);
		}

		private Ray GetDirectionRay(Transform target)
		{
			return new Ray()
			{
				origin = _transform.position + VIEW_POINT_DELTA,
				direction = target.position - _transform.position
			};
		}
	}
}