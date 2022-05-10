using Assets.Crimson.Core.AI;
using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.AI
{
	[Serializable, HideMonoScript]
	public class FollowLeaderBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority BasePriority = new BasePriority
		{
			Value = 1
		};

		public EvaluatedCurve CurvePriority = new EvaluatedCurve(0);
		public TagFilter TagFilter;

		private const float FINISH_BEHAVIOUR_DISTSQ = 20f;
		private AIPathControl _path;
		private Transform _target = null;
		private Transform _transform = null;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
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

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_transform = Actor.GameObject.transform;
			_target = targets.FirstOrDefault(t => TagFilter.Filter(t) && t != _transform);

			if (_target == null)
			{
				return 0;
			}

			var distance = math.distance(_transform.position, _target.position);
			var result = CurvePriority.Evaluate(distance) * BasePriority.Value;

			return result;
		}

		public void Execute()
		{
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			_path.SetTarget(_target);
			return true;
		}

		public void DrawGizmos()
		{
			if (_target == null)
			{
				return;
			}

			Gizmos.color = Color.green;
			var targetPosition = _path.EndWaypointPosition;
			Gizmos.DrawLine(_transform.position, targetPosition);
			Gizmos.DrawSphere(targetPosition, .5f);
		}
	}
}