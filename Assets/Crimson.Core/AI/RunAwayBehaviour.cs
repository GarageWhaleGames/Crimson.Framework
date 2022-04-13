using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Crimson.Core.AI
{
	[Serializable]
	public class RunAwayBehaviour : IAIBehaviour
	{
		public string XAxis => "current health";

		public string[] AdditionalModes => new string[0];

		public bool NeedCurve => true;
		public bool NeedTarget => false;
		public bool NeedActions => false;

		public bool HasDistanceLimit => false;

		private const float FINISH_ROAM_DISTSQ = 2f;
		private const float PRIORITY_MULTIPLIER = 0.5f;

		private AIBehaviourSetting _behaviour = null;
		private AbilityActorPlayer _player = null;
		private Transform _transform = null;
		private readonly NavMeshPath _path = new NavMeshPath();

		private int _currentWaypoint = 0;

		public float Evaluate(Entity entity, AIBehaviourSetting behaviour, AbilityAIInput ai, List<Transform> targets)
		{
			_behaviour = behaviour;
			_transform = _behaviour.Actor.GameObject.transform;
			_player = _behaviour.Actor.GameObject.GetComponent<AbilityActorPlayer>();

			if (_player == null) return 0f;

			var health = _player.CurrentHealth;
			var sampleScale = behaviour.curveMaxSample - behaviour.curveMinSample;
			var curveSample = math.clamp(
				(health - behaviour.curveMinSample) / sampleScale, 0f, 1f);
			return behaviour.priorityCurve.Evaluate(curveSample) * PRIORITY_MULTIPLIER;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			Vector3 target;
			float distSq;

			_path.ClearCorners();

			_currentWaypoint = 1;

			do
			{
				target = NavMeshRandomPointUtil.GetRandomLocation();
				distSq = math.distancesq(_transform.position, target);
			} while (distSq < FINISH_ROAM_DISTSQ);

			var result = NavMesh.CalculatePath(_transform.position, target, NavMesh.AllAreas, _path);

			return result;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (_path.status == NavMeshPathStatus.PathInvalid) return false;

			var distSq = math.distancesq(_transform.position, _path.corners[_currentWaypoint]);

			if (distSq <= Constants.WAYPOINT_SQDIST_THRESH)
			{
				_currentWaypoint++;
			}

			if (_currentWaypoint >= _path.corners.Length ||
				_currentWaypoint == _path.corners.Length - 1 && distSq < FINISH_ROAM_DISTSQ)
			{
				inputData.Move = float2.zero;
				return false;
			}

			var dir = math.normalize(_path.corners[_currentWaypoint] - _transform.position);

			inputData.Move = new float2(dir.x, dir.z);

			return true;
		}
	}
}