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

namespace Crimson.Core.AI
{
	[Serializable]
	public class FollowLeaderBehaviour : IAIBehaviour
	{
		private const float FINISH_BEHAVIOUR_DISTSQ = 20f;
		private readonly NavMeshPath _path = new NavMeshPath();
		private AIBehaviourSetting _behaviour = null;
		private int _currentWaypoint = 0;
		private Transform _target = null;
		private Transform _transform = null;
		public string[] AdditionalModes => new string[0];
		public bool NeedActions => false;
		public bool NeedCurve => true;
		public bool NeedTarget => true;
		public string XAxis => "";

		public bool HasDistanceLimit => false;

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (_path.status == NavMeshPathStatus.PathInvalid || _transform == null)
			{
				return false;
			}

			var distSq = math.distancesq(_transform.position, _path.corners[_currentWaypoint]);

			if (distSq <= Constants.WAYPOINT_SQDIST_THRESH)
			{
				_currentWaypoint++;
			}

			if ((_currentWaypoint == _path.corners.Length - 1 && distSq < FINISH_BEHAVIOUR_DISTSQ) || _currentWaypoint >= _path.corners.Length)
			{
				inputData.Move = float2.zero;
				return false;
			}

			var dir = math.normalize(_path.corners[_currentWaypoint] - _transform.position);

			inputData.Move = new float2(dir.x, dir.z);

			return true;
		}

		public float Evaluate(Entity entity, AIBehaviourSetting behaviour, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_behaviour = behaviour;
			_transform = _behaviour.Actor.GameObject.transform;
			_target = targets.FirstOrDefault(t => t.FilterTag(_behaviour) && t != _transform);

			if (_target == null)
			{
				return 0;
			}

			var dist = math.distance(_transform.position, _target.position);
			var sampleScale = _behaviour.curveMaxSample - _behaviour.curveMinSample;
			var curveSample = math.clamp(
				(dist - _behaviour.curveMinSample) / sampleScale, 0f, 1f);
			var result = _behaviour.priorityCurve.Evaluate(curveSample) * _behaviour.basePriority;

			return result;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			_path.ClearCorners();

			_currentWaypoint = 1;

			var target = _target.position;

			var result = NavMesh.CalculatePath(_transform.position, target, NavMesh.AllAreas, _path);

			return result;
		}
	}
}