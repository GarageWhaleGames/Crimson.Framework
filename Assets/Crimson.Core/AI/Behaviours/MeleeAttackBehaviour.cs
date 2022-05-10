using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.AI;
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

namespace Assets.Crimson.Core.AI.Behaviours
{
	[Serializable, HideMonoScript]
	public class MeleeAttackBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public IActor Actor { get; set; }

		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public EvaluatedCurve CurvePriority = new EvaluatedCurve(0)
		{
			XAxisTooltip = "distance to closest seen target"
		};

		public ExecutableCustomInput ExecutableCustomInput;
		public string AttackAnimationName;
		public TagFilter TagFilter;

		public float MaxDistance = 1.5f;

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

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
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

		public void Execute()
		{
		}

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_transform = Actor.GameObject.transform;

			if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity)
				|| CustomInput == null
				|| !CustomInput.bindingsDict.TryGetValue(ExecutableCustomInput.CustomInputIndex, out _abilities)
				|| !_abilities.ActionPossible())
			{
				return 0f;
			}

			var orderedTargets = targets
				.Where(t => TagFilter.Filter(t) && t != _transform)
				.OrderBy(t => math.distance(_transform.position, t.position));

			foreach (var t in orderedTargets)
			{
				if (Physics.Raycast(GetDirectionRay(t), out var hit,
					MaxDistance))
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
			if (_target == null
				|| math.distance(_transform.position, _target.position) > MaxDistance
				|| !_abilities.ActionPossible())
			{
				return false;
			}

			if (Physics.Raycast(GetDirectionRay(_target), out var hit, MaxDistance) && hit.transform == _target)
			{
				inputData.CustomInput[ExecutableCustomInput.CustomInputIndex] = 1f;
				return true;
			}

			return false;
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