using Assets.Crimson.Core.AI;
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

		private const float FINISH_ROAM_DISTSQ = 2f;
		private const float PRIORITY_MULTIPLIER = 0.5f;

		private Transform _transform = null;
		private readonly AIPathControl _path = new AIPathControl(finishThreshold: FINISH_ROAM_DISTSQ);

		[ShowInInspector, ReadOnly] private Vector3 _target;

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_transform = Actor.GameObject.transform;

			return Random.value * Priority.Value * PRIORITY_MULTIPLIER;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			float distSq;

			do
			{
				_target = NavMeshRandomPointUtil.GetRandomLocation();
				distSq = math.distancesq(_transform.position, _target);
			} while (distSq < FINISH_ROAM_DISTSQ);

			return _path.Setup(_transform, _target);
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