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

namespace Assets.Crimson.Core.AI.Behaviours
{
	[Serializable, HideMonoScript]
	public class AimSnipeBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public EvaluatedCurve CurvePriority = new EvaluatedCurve(0)
		{
			XAxisTooltip = "distance to closest seen target"
		};

		public TagFilter TagFilter;

		public ExecutableCustomInput ExecutableCustomInput;

		[InfoBox("Time before shoot (seconds)")]
		public float AttackDelay = 1f;

		public float SightRadius = 20;
		public float SearchTime = .25f;
		private Transform _target = null;
		private Transform _weaponRoot;
		private SearchingLaser _searchingLaser;
		private Transform _transform = null;
		private readonly DelayTimer _delayTimer = new DelayTimer();

		public IActor Actor { get; set; }
		private readonly Vector3 VIEW_POINT_DELTA = new Vector3(0f, 1.5f, 0f);
		private const float SHOOTING_ANGLE_THRESH = 1f;
		private const float AIM_MAX_DIST = 40f;
		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			var result = true;
			var direction = _target.position - _transform.position;
			if (Physics.Raycast(GetRayTo(_target), out var hit,
				AIM_MAX_DIST) && hit.transform == _target)
			{
				var targetRotation = Quaternion.FromToRotation(_transform.forward, direction);
				var angle = math.abs(targetRotation.eulerAngles.y);
				var maxAngle = CalculateMaxAngle();
				_searchingLaser.MaxAngle = maxAngle;
				if (angle >= SHOOTING_ANGLE_THRESH)
				{
					_searchingLaser.Run();
					_delayTimer.Stop();
					var lookInput = new float2
					{
						x = targetRotation.y * targetRotation.w,
						y = targetRotation.x * targetRotation.w,
					};
					inputData.Look = lookInput.x == 0 && lookInput.y == 0 ?
						float2.zero
						: math.normalize(lookInput);
				}
				else if (_delayTimer.IsStopped)
				{
					_delayTimer.Start(AttackDelay);
					inputData.Look = float2.zero;
				}

				if (_delayTimer.IsExpire && _searchingLaser.IsStopped)
				{
					_delayTimer.Stop();
					inputData.CustomInput[ExecutableCustomInput.CustomInputIndex] = 1;
					_searchingLaser.Run();
				}
			}
			else
			{
				result = false;
			}

			_searchingLaser.Update();

			return result;
		}
		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<DeadActorTag>(entity))
			{
				return 0f;
			}
			_target = null;
			_transform = Actor.GameObject.transform;

			var orderedTargets = targets
				.Where(t => TagFilter.Filter(t) && t != _transform)
				.OrderBy(t => math.distancesq(_transform.position, t.position));

			foreach (var target in orderedTargets)
			{
				var direction = target.position - _transform.position;
				if (Physics.Raycast(GetRayTo(target), out var hit,
					AIM_MAX_DIST))
				{
					if (hit.transform != target)
					{
						continue;
					}

					_target = target;
					var distance = math.distance(_transform.position, _target.position);
					var result = CurvePriority.Evaluate(distance) * Priority.Value;

					return result;
				}
			}

			return 0f;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			var weapon = Actor.Abilities.Find(s => s is AbilityWeapon) as AbilityWeapon;
			if (weapon == null)
			{
				return false;
			}
			_weaponRoot = weapon.SpawnPointsRoot;
			_searchingLaser.Target = _weaponRoot;
			return true;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_searchingLaser = new SearchingLaser(SightRadius, SearchTime);
		}

		public void Execute()
		{
		}

		public void DrawGizmos()
		{
			DrawRay(_transform, Color.red);
			DrawRay(_weaponRoot, Color.yellow);
			DrawTarget(_target, Color.green);
		}

		private float CalculateMaxAngle()
		{
			var direction = _target.position - _transform.position;
			var right = (Quaternion.AngleAxis(90, Vector3.up) * direction).normalized;
			var maxRightDirection = direction + (right * SightRadius);
			return Vector3.Angle(direction, maxRightDirection);
		}

		private Ray GetRayTo(Transform target)
		{
			var origin = _transform.position + VIEW_POINT_DELTA;
			var direction = target.position - origin;
			return new Ray(origin, direction);
		}

		private void DrawTarget(Transform target, Color color)
		{
			if (target == null)
			{
				return;
			}

			Gizmos.color = color;
			Gizmos.matrix = target.localToWorldMatrix;
			Gizmos.DrawWireSphere(Vector3.zero, SightRadius);
		}

		private void DrawRay(Transform transform, Color color)
		{
			if (transform == null)
			{
				return;
			}

			var rayLength = 40;
			var raySize = .2f;
			Gizmos.color = color;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(VIEW_POINT_DELTA + Vector3.forward * rayLength / 2, new Vector3(raySize, raySize, rayLength));
		}
	}
}