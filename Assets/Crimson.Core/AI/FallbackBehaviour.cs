using Assets.Crimson.Core.Utils;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.AI
{
	public class FallbackBehaviour : IAIBehaviour
	{
		public Vector3 _fallbackPlace;
		public Vector3[] positions = new Vector3[MaxPoints];
		private const int MaxPoints = 16;
		private const float PRIORITY_MULTIPLIER = 0.5f;
		private readonly NavMeshPath _path = new NavMeshPath();
		private AIBehaviourSetting _behaviour;
		private int _currentWaypoint = 0;
		private Transform _target = null;
		private Transform _transform = null;

		public string[] AdditionalModes => new[]
			{"Strict mode: distance to priority", "Random mode: priority as probability"};

		public bool HasDistanceLimit => true;
		public bool NeedActions => false;
		public bool NeedCurve => true;
		public bool NeedTarget => true;
		public string XAxis => "Target priority based on distance to it";

		private float Priority
		{
			get
			{
				return math.distancesq(_transform.position, _fallbackPlace) <= .5f ? 0f :
					_behaviour.basePriority * PRIORITY_MULTIPLIER;
			}
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (_path.status == NavMeshPathStatus.PathInvalid)
			{
				return false;
			}

			var distSq = math.distancesq(_transform.position, _path.corners[_currentWaypoint]);

			if (distSq <= Constants.WAYPOINT_SQDIST_THRESH)
			{
				_currentWaypoint++;
			}

			if (_currentWaypoint >= _path.corners.Length)
			{
				inputData.Move = float2.zero;
				return false;
			}

			var dir = _path.corners[_currentWaypoint] - _transform.position;

			inputData.Move = math.normalize(new float2(dir.x, dir.z));

			return true;
		}

		public float Evaluate(Entity entity, AIBehaviourSetting behaviour, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_behaviour = behaviour;
			_transform = behaviour.Actor?.GameObject.transform;

			if (_transform == null)
			{
				return 0f;
			}

			var filteredTargets = targets.Where(t => t.FilterTag(behaviour) && t != _transform).ToList();
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

			var sampleScale = behaviour.curveMaxSample - behaviour.curveMinSample;

			switch (behaviour.additionalMode)
			{
				case "Random mode: priority as probability":
					var priorities = new List<MinMaxTarget>();

					var priorityCache = 0f;

					foreach (var target in filteredTargets)
					{
						var d = math.distance(_transform.position, target.position);
						var curveSample = math.clamp(
							(d - behaviour.curveMinSample) / sampleScale, 0f, 1f);
						var priority = behaviour.priorityCurve.Evaluate(curveSample);

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
				case "Strict mode: distance to priority":
					var orderedTargets = filteredTargets.OrderBy(t =>
					{
						var d = math.distance(_transform.position, t.position);
						var curveSample = math.clamp(
							(d - behaviour.curveMinSample) / sampleScale, 0f, 1f);
						return behaviour.priorityCurve.Evaluate(curveSample);
					}).ToList();

					_target = orderedTargets.Last();
					break;
			}

			_fallbackPlace = CalculateBestPosition(_target);
			return Priority;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			_path.ClearCorners();

			if (_target == null || _transform == null)
			{
				return false;
			}

			_currentWaypoint = 1;
			_fallbackPlace = CalculateBestPosition(_target);
			var result = NavMesh.CalculatePath(_transform.position, _fallbackPlace, NavMesh.AllAreas, _path);

			return result;
		}

		internal void DrawGizmosSelected()
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
			positions = NavMeshUtils.CalculatePositionsOnCircle(sourcePosition, _behaviour.LimitDistance, MaxPoints);
			var bestPosition = Vector3.zero;
			var minimalPathLength = float.MaxValue;
			for (var i = 0; i < positions.Length; i++)
			{
				var position = positions[i];
				if (position == Vector3.zero)
				{
					continue;
				}

				var hasPath = NavMesh.CalculatePath(_transform.position, position, NavMesh.AllAreas, _path);
				if (!hasPath)
				{
					continue;
				}

				var pathLength = _path.Length();
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