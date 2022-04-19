using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.AI
{
	[Serializable, HideMonoScript]
	public class AimBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public CurvePriority CurvePriority = new CurvePriority(0)
		{
			XAxisTooltip = "distance to closest seen target"
		};

		public TagFilter TagFilter;

		public ExecutableCustomInput ExecutableCustomInput;

		[InfoBox("Time before shoot (seconds)")]
		public float AttackDelay = 1f;

		private const float AIM_MAX_DIST = 40f;
		private readonly Vector3 VIEW_POINT_DELTA = new Vector3(0f, 0.6f, 0f);
		private const float SHOOTING_ANGLE_THRESH = 1f;
		private Transform _target = null;
		private Transform _transform = null;

		private float _catchingAimTime;

		public IActor Actor { get; set; }

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			var result = false;
			var direction = _target.position - _transform.position;
			if (Physics.Raycast(_transform.position + VIEW_POINT_DELTA, direction, out var hit,
				AIM_MAX_DIST) && hit.transform == _target)
			{
				var targetRotation = Quaternion.FromToRotation(_transform.forward, direction);
				var angle = targetRotation.eulerAngles.y;
				if (angle >= SHOOTING_ANGLE_THRESH)
				{
					_catchingAimTime = -1;
					var lookInput = new float2
					{
						x = targetRotation.y * targetRotation.w,
						y = targetRotation.x * targetRotation.w,
					};
					inputData.Look = lookInput.x == 0 && lookInput.y == 0 ?
						float2.zero
						: math.normalize(lookInput);
					result = true;
				}
				else if (_catchingAimTime == -1)
				{
					_catchingAimTime = Time.realtimeSinceStartup;
					result = true;
				}

				if (Time.realtimeSinceStartup - _catchingAimTime > AttackDelay)
				{
					result = true;
				}
				else
				{
					_catchingAimTime = -1;
					inputData.CustomInput[ExecutableCustomInput.CustomInputIndex] = 1;
				}
			}

			return result;
		}

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_transform = Actor.GameObject.transform;

			if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity))
			{
				return 0f;
			}

			var orderedTargets = targets
				.Where(t => TagFilter.Filter(t) && t != _transform)
				.OrderBy(t => math.distancesq(_transform.position, t.position)).ToList();

			if (orderedTargets.Count == 0)
			{
				return 0f;
			}

			foreach (var t in orderedTargets)
			{
				var direction = t.position - _transform.position;
				if (Physics.Raycast(_transform.position + VIEW_POINT_DELTA, direction, out var hit,
					AIM_MAX_DIST))
				{
					if (hit.transform != t)
					{
						continue;
					}

					_target = t;
					var angle = Vector3.Angle(_transform.forward, direction);
					if (angle < SHOOTING_ANGLE_THRESH)
					{
						continue;
					}

					var distance = math.distance(_transform.position, _target.position);
					var result = CurvePriority.Evaluate(distance) * Priority.Value;

					return result;
				}
			}

			return 0f;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
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
			var rayLength = 40;
			var raySize = .2f;
			Gizmos.color = Color.red;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.forward * rayLength / 2, new Vector3(raySize, raySize, rayLength));
		}
	}
}