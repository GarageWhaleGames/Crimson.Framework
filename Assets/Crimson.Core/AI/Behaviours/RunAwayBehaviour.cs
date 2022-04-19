using Assets.Crimson.Core.AI;
using Assets.Crimson.Core.AI.GeneralParams;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.AI
{
	[HideMonoScript]
	public class RunAwayBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public CurvePriority CurvePriority = new CurvePriority(0)
		{
			XAxisTooltip = "current health"
		};

		public IActor Actor { get; set; }

		private const float FINISH_ROAM_DISTSQ = 2f;
		private const float PRIORITY_MULTIPLIER = 0.5f;

		private AbilityActorPlayer _player = null;
		private Transform _transform = null;
		private readonly AIPathControl _path = new AIPathControl(finishThreshold: FINISH_ROAM_DISTSQ);

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			var actorObject = Actor.GameObject;
			_transform = actorObject.transform;
			_player = actorObject.GetComponent<AbilityActorPlayer>();

			if (_player == null)
			{
				return 0f;
			}

			var health = _player.CurrentHealth;
			return CurvePriority.Evaluate(health) * PRIORITY_MULTIPLIER;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			Vector3 target;
			float distSq;

			do
			{
				target = NavMeshRandomPointUtil.GetRandomLocation();
				distSq = math.distancesq(_transform.position, target);
			} while (distSq < FINISH_ROAM_DISTSQ);

			return _path.Setup(_transform, target);
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
	}
}