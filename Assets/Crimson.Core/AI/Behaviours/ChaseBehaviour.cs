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
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public EvaluatedCurve CurvePriority = new EvaluatedCurve(0)
		{
			XAxisTooltip = "Target priority based on distance to it"
		};

		public EvaluationMode EvaluationMode;
		public TagFilter TagFilter;

		private const float PRIORITY_MULTIPLIER = 0.5f;

		private AIPathControl _path;
		private Transform _target = null;

		private Transform _transform = null;
		public IActor Actor { get; set; }

		private Transform Target
		{
			get => _target;
			set
			{
				_target = value;
				_path.SetTarget(value);
			}
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_path = new AIPathControl(transform);
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			_path.SetTarget(_target.position);
			inputData.Move = _path.MoveDirection;
			return !_path.HasArrived;
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
			if (filteredTargets.Count == 0)
			{
				return 0f;
			}

			if (filteredTargets.Count == 1)
			{
				target = filteredTargets.First();
				Target = target;
				return _path.HasArrived
					? 0f :
					Priority.Value * PRIORITY_MULTIPLIER;
			}

			var sampleScale = CurvePriority.SampleScale;

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
			Target = target;
			return _path.HasArrived ?
				0f :
				Priority.Value * PRIORITY_MULTIPLIER;
		}

		public void Execute()
		{
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			return true;
		}

		private float CalculatePriorityFor(Vector3 position)
		{
			var distance = math.distance(_transform.position, position);
			return CurvePriority.Evaluate(distance);
		}

		public void DrawGizmos()
		{
			if (Target == null || _path == null || !_path.IsValid)
			{
				return;
			}

			Gizmos.color = Color.green;
			var targetPosition = _path.EndWaypointPosition;
			DrawTarget(targetPosition, Color.green);
			DrawTarget(_path.NextPosition, Color.yellow);
		}

		private void DrawTarget(Vector3 postion, Color color)
		{
			Gizmos.color = color;
			Gizmos.DrawLine(_transform.position, postion);
			Gizmos.DrawSphere(postion, .5f);
		}
	}
}