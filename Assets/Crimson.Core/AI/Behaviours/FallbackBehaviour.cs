using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Assets.Crimson.Core.Utils;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.AI
{
	[HideMonoScript]
	public class FallbackBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority BasePriority = new BasePriority
		{
			Value = 1
		};

		public CurvePriority CurvePriority = new CurvePriority(0)
		{
			XAxisTooltip = "Target priority based on distance to it"
		};

		public EvaluationMode EvaluationMode;

		public DistanceLimitation DistanceLimitation = new DistanceLimitation
		{
			MaxDistance = 10
		};

		public TagFilter TagFilter;

		private Vector3 _fallbackPlace;
		private Vector3[] positions = new Vector3[MaxPoints];
		private const int MaxPoints = 16;
		private NavMeshPath _path;
		private int _currentWaypoint = 0;
		private Transform _target = null;
		private Transform _transform = null;

		public IActor Actor { get; set; }

		private float Priority
		{
			get
			{
				var distanceToPlace = math.distancesq(_transform.position, _fallbackPlace);
				var distanceToTarget = math.distancesq(_transform.position, _target.position);
				return distanceToPlace <= .5f ? 0f :
					CurvePriority.Evaluate(distanceToTarget) * BasePriority.Value;
			}
		}

		public NavMeshPath Path
		{
			get
			{
				if (_path == null)
				{
					_path = new NavMeshPath();
				}
				return _path;
			}
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (Path.status == NavMeshPathStatus.PathInvalid)
			{
				return false;
			}

			var distance = 0.0f;
			if (Path.corners.Length > _currentWaypoint)
			{
				distance = math.distancesq(_transform.position, Path.corners[_currentWaypoint]);
			}

			if (distance <= Constants.WAYPOINT_SQDIST_THRESH)
			{
				_currentWaypoint++;
			}

			if (_currentWaypoint >= Path.corners.Length)
			{
				inputData.Move = float2.zero;
				return false;
			}

			var dir = Path.corners[_currentWaypoint] - _transform.position;

			inputData.Move = math.normalize(new float2(dir.x, dir.z));

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
				_fallbackPlace = CalculateBestPosition(_target);
				return Priority;
			}

			var sampleScale = CurvePriority.SampleScale;

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

			_fallbackPlace = CalculateBestPosition(_target);
			return Priority;
		}

		public void Execute()
		{
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			Path.ClearCorners();

			if (_target == null || _transform == null)
			{
				return false;
			}

			_currentWaypoint = 1;
			_fallbackPlace = CalculateBestPosition(_target);
			var result = NavMesh.CalculatePath(_transform.position, _fallbackPlace, NavMesh.AllAreas, Path);

			return result;
		}

		public void DrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(_fallbackPlace, .2f);
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
			var bestPosition = Vector3.zero;
			var minimalPathLength = float.MaxValue;
			for (var i = 0; i < positions.Length; i++)
			{
				var position = positions[i];
				if (position == Vector3.zero)
				{
					continue;
				}

				var hasPath = NavMesh.CalculatePath(_transform.position, position, NavMesh.AllAreas, Path);
				if (!hasPath)
				{
					continue;
				}

				var pathLength = Path.Length();
				if (pathLength < minimalPathLength)
				{
					bestPosition = position;
					minimalPathLength = pathLength;
				}
			}

			return bestPosition == Vector3.zero ? _transform.position : bestPosition;
		}
	}
}