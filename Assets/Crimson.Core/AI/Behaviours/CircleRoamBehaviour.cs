using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Assets.Crimson.Core.Utils;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.AI
{
	[HideMonoScript]
	public class CircleRoamBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority BasePriority = new BasePriority
		{
			Value = 1
		};

		public CurvePriority CurvePriority = new CurvePriority(0)
		{
			XAxisTooltip = "Target priority based on distance to it"
		};

		public DistanceLimitation DistanceLimitation = new DistanceLimitation
		{
			MaxDistance = 5
		};

		public EvaluationMode EvaluationMode;

		public TagFilter TagFilter;

		private Vector3[] positions = new Vector3[MaxPoints];
		private const int MaxPoints = 16;
		private const float PositionThreshold = .5f;
		private const float PRIORITY_MULTIPLIER = 0.5f;
		private Transform _target;
		private Vector3 _targetPosition;
		private Transform _transform = null;

		private readonly AIPathControl _path = new AIPathControl(finishThreshold: PositionThreshold);

		private float Priority
		{
			get
			{
				return math.distancesq(_transform.position, _targetPosition) <= PositionThreshold ?
					0f :
					UnityEngine.Random.value * BasePriority.Value * PRIORITY_MULTIPLIER;
			}
		}

		public IActor Actor { get; set; }

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
			inputData.Move = new float2(dir.x, dir.z);

			return true;
		}

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_transform = Actor.GameObject.transform;

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
				_target = filteredTargets.First();
				_targetPosition = CalculateBestPosition(_target);
				return Priority;
			}

			switch (EvaluationMode)
			{
				case EvaluationMode.Random:
					var priorities = new List<MinMaxTarget>();

					var priorityCache = 0f;

					foreach (var target in filteredTargets)
					{
						var distance = math.distance(_transform.position, target.position);
						var priority = CurvePriority.Evaluate(distance);

						priorities.Add(new MinMaxTarget
						{
							Min = priorityCache,
							Max = priority + priorityCache,
							Target = target
						});

						priorityCache += priority;
					}

					var randomNumber = UnityEngine.Random.Range(0f, priorityCache);

					_target = priorities.Find(t => t.Min < randomNumber && t.Max >= randomNumber).Target;
					break;

				default: // ReSharper disable once RedundantCaseLabel
				case EvaluationMode.Strict:
					var orderedTargets = filteredTargets.OrderBy(t =>
					{
						var distance = math.distance(_transform.position, t.position);
						return CurvePriority.Evaluate(distance);
					}).ToList();

					_target = orderedTargets.Last();
					break;
			}

			_targetPosition = CalculateBestPosition(_target);
			return Priority;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			_targetPosition = CalculateBestPosition(_target);
			return _path.Setup(_transform, _targetPosition);
		}

		public void DrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(_path.EndWaypointPosition, .2f);
			Gizmos.color = Color.yellow;
			var points = positions;
			for (var i = 0; i < points.Length; i++)
			{
				Gizmos.DrawWireSphere(points[i], .2f);
			}
		}

		private Vector3 CalculateBestPosition(Transform target)
		{
			var sourcePosition = target.position;
			positions = NavMeshUtils.CalculatePositionsOnCircle(sourcePosition, DistanceLimitation.MaxDistance, MaxPoints);
			var positionStats = new Tuple<float, Vector3>[MaxPoints];
			var path = new NavMeshPath();
			for (var i = 0; i < positions.Length; i++)
			{
				var position = positions[i];
				if (position == Vector3.zero
					|| !NavMesh.CalculatePath(_transform.position, position, NavMesh.AllAreas, path))
				{
					positionStats[i] = new Tuple<float, Vector3>(float.MaxValue, position);
					continue;
				}

				var pathLength = path.Length();
				positionStats[i] = new Tuple<float, Vector3>(pathLength, position);
			}

			var topPositions = positionStats.OrderBy(s => s.Item1).Skip(1).Take(2);
			var bestPosition = UnityEngine.Random.value > .5f ? topPositions.ElementAt(0) : topPositions.ElementAt(1);
			return bestPosition.Item1 != float.MaxValue ? bestPosition.Item2 : _transform.position;
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