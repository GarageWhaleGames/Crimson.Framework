using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[Serializable, HideMonoScript]
	public class RoamBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public IActor Actor { get; set; }

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
			set => _path = value;
		}

		private const float FINISH_ROAM_DISTSQ = 2f;
		private const float PRIORITY_MULTIPLIER = 0.5f;

		private Transform _transform = null;
		private NavMeshPath _path;

		private int _currentWaypoint = 0;
		[ShowInInspector, ReadOnly] private Vector3 _target;

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_transform = Actor.GameObject.transform;

			return Random.value * Priority.Value * PRIORITY_MULTIPLIER;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			float distSq;

			Path.ClearCorners();

			_currentWaypoint = 1;

			do
			{
				_target = NavMeshRandomPointUtil.GetRandomLocation();
				distSq = math.distancesq(_transform.position, _target);
			} while (distSq < FINISH_ROAM_DISTSQ);

			var result = NavMesh.CalculatePath(_transform.position, _target, NavMesh.AllAreas, Path);

			return result;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (Path.status == NavMeshPathStatus.PathInvalid
				|| _transform == null)
			{
				return false;
			}

			var distSq = 0.0f;
			if (Path.corners.Length > _currentWaypoint)
			{
				distSq = math.distancesq(_transform.position, Path.corners[_currentWaypoint]);
			}

			if (distSq <= Constants.WAYPOINT_SQDIST_THRESH)
			{
				_currentWaypoint++;
			}

			if ((_currentWaypoint == Path.corners.Length - 1 && distSq < FINISH_ROAM_DISTSQ) || _currentWaypoint >= Path.corners.Length)
			{
				inputData.Move = float2.zero;
				return false;
			}

			var dir = math.normalize(Path.corners[_currentWaypoint] - _transform.position);

			inputData.Move = new float2(dir.x, dir.z);

			return true;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
		}

		public void DrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(_transform.position, _target);
			Gizmos.DrawSphere(_target, .2f);
		}
	}
}