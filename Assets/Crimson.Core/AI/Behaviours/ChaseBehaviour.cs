using Assets.Crimson.Core.AI;
using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.Common;
using Crimson.Core.Components;
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
	public class ChaseBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public EvaluatedCurve CurvePriority = new EvaluatedCurve(0)
		{
			XAxisTooltip = "Target priority based on distance to it"
		};

		public EvaluationMode EvaluationMode;

		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public TagFilter TagFilter;

		private const float FINISH_CHASE_DISTSQ = 1f;
		private const float PRIORITY_MULTIPLIER = 0.5f;
		private readonly AIPathControl _path = new AIPathControl(finishThreshold: FINISH_CHASE_DISTSQ);
		private Transform _target = null;
		private Transform _transform = null;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (!_path.IsValid)
			{
				return false;
			}

			_path.NextPoint();

			if (_path.HasArrived)
			{
				inputData.Move = float2.zero;
				return false;
			}

			var dir = _path.Direction;

			inputData.Move = math.normalize(new float2(dir.x, dir.z));

			return true;
		}

		public void DrawGizmos()
		{
			if (_target == null)
			{
				return;
			}

			Gizmos.color = Color.green;
			var targetPosition = _path.EndWaypointPosition;
			Gizmos.DrawLine(_transform.position, targetPosition);
			Gizmos.DrawSphere(targetPosition, .5f);
		}

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			Transform target = null;
			_transform = Actor?.GameObject.transform;

			if (_transform == null)
			{
				return 0f;
			}

			var filteredTargets = targets.Where(t => TagFilter.Filter(t) && t != _transform).ToList();
			switch (filteredTargets.Count)
			{
				case 0:
					break;

				case 1:
					target = filteredTargets.First();
					break;

				default:
					target = SelectTarget(filteredTargets);
					break;
			}

			if (target == null)
			{
				return 0;
			}
			_target = target;
			var distance = math.distancesq(_transform.position, target.position);
			return distance < FINISH_CHASE_DISTSQ ?
				0f :
				Priority.Value * PRIORITY_MULTIPLIER;
		}

		public void Execute()
		{
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			return _path.Setup(_transform, _target);
		}

		private Transform SelectTarget(List<Transform> filteredTargets)
		{
			Transform target;
			switch (EvaluationMode)
			{
				case EvaluationMode.Random:
					var priorities = new List<MinMaxTarget>();

					var priorityCache = 0f;

					foreach (var item in filteredTargets)
					{
						var priority = CalculatePriorityFor(item.position);

						priorities.Add(new MinMaxTarget
						{
							Min = priorityCache,
							Max = priority + priorityCache,
							Target = item
						});

						priorityCache += priority;
					}

					var randomNumber = Random.Range(0f, priorityCache);

					target = priorities.Find(t => t.Min < randomNumber && t.Max >= randomNumber).Target;
					break;

				default: // ReSharper disable once RedundantCaseLabel
				case EvaluationMode.Strict:
					var orderedTargets = filteredTargets
						.OrderBy(t => CalculatePriorityFor(t.position))
						.ToList();

					target = orderedTargets.Last();
					break;
			}

			return target;
		}

		private float CalculatePriorityFor(Vector3 position)
		{
			var distance = math.distance(_transform.position, position);
			return CurvePriority.Evaluate(distance);
		}
	}
}