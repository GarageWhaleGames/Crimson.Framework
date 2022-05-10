using Assets.Crimson.Core.AI;
using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.AI
{
	[HideMonoScript]
	public class LimitedRoamBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public DistanceLimitation DistanceLimitation = new DistanceLimitation
		{
			MaxDistance = 10
		};

		public IActor Actor { get; set; }

		private const float FINISH_ROAM_DISTSQ = 2f;

		private Transform _transform = null;
		private AIPathControl _path;

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_transform = Actor.GameObject.transform;

			return Random.value * Priority.Value;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			Vector3 target;
			float distSq;

			do
			{
				target = NavMeshRandomPointUtil.GetRandomLocation(DistanceLimitation.MaxDistance);
				distSq = math.distancesq(_transform.position, target);
			} while (distSq < FINISH_ROAM_DISTSQ);

			_path.SetTarget(target);
			return true;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (!_path.IsValid)
			{
				return false;
			}

			if (_path.HasArrived)
			{
				inputData.Move = float2.zero;
				return false;
			}

			return _path.IsValid;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_path = new AIPathControl(transform);
		}

		public void Execute()
		{
		}

		public void DrawGizmos()
		{
			Gizmos.color = Color.green;
			var targetPosition = _path.EndWaypointPosition;
			Gizmos.DrawLine(_transform.position, targetPosition);
			Gizmos.DrawSphere(targetPosition, .5f);
		}
	}
}