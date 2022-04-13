using Assets.Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[Serializable]
	public class ChaseBehaviour : IAIBehaviour
	{
		public string XAxis => "Target priority based on distance to it";

		public string[] AdditionalModes => new[]
			{"Strict mode: distance to priority", "Random mode: priority as probability"};

		public bool NeedCurve => true;
		public bool NeedTarget => true;
		public bool NeedActions => false;

		public bool HasDistanceLimit => false;

		private const float FINISH_CHASE_DISTSQ = 5f;
		private const float PRIORITY_MULTIPLIER = 0.5f;

		private Transform _target = null;
		private Transform _transform = null;
		private readonly NavMeshPath _path = new NavMeshPath();

		private int _currentWaypoint = 0;

		public float Evaluate(Entity entity, AIBehaviourSetting behaviour, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
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
				return math.distancesq(_transform.position, _target.position) < FINISH_CHASE_DISTSQ ? 0f :
					behaviour.basePriority * PRIORITY_MULTIPLIER;
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

					var randomNumber = Random.Range(0f, priorityCache);

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

			return math.distancesq(_transform.position, _target.position) < FINISH_CHASE_DISTSQ ? 0f :
				behaviour.basePriority * PRIORITY_MULTIPLIER;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			_path.ClearCorners();

			if (_target == null || _transform == null)
			{
				return false;
			}

			_currentWaypoint = 1;
			var result = NavMesh.CalculatePath(_transform.position, _target.position, NavMesh.AllAreas, _path);

			return result;
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
	}
}