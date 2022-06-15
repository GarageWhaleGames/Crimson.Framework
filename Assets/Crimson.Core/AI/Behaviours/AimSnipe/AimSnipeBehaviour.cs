using Assets.Crimson.Core.AI.AimLaserFX;
using Assets.Crimson.Core.AI.Behaviours.AimSnipe.States;
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

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe
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
		internal Transform _target = null;
		private Transform _weaponRoot;
		internal SearchingLaser _searchingLaser;
		internal Transform _transform = null;
		private Vector3 _lastPosition;
		private const float PositionThreshold = .1f;

		public IActor Actor { get; set; }
		private readonly Vector3 VIEW_POINT_DELTA = new Vector3(0f, 1.5f, 0f);
		private const float AIM_MAX_DIST = 40f;

		private bool TargetInRange => Physics.Raycast(GetRayTo(_target), out var hit,
				AIM_MAX_DIST) && hit.transform == _target;

		internal Vector3 TargetDirection => _target.position - _transform.position;

		private readonly IStateContext _stateContext = new StateContext();

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			return !TargetInRange || HandleStates(entity, dstManager, ref inputData);
		}

		private bool HandleStates(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (!(_stateContext.Current is Aiming) && math.distancesq(_target.position, _lastPosition) >= PositionThreshold)
			{
				_lastPosition = _target.position;
				_stateContext.TransitionTo(new Aiming(this));
			}
			else if (_stateContext.Current.IsDone)
			{
				if (_stateContext.Current is Aiming)
				{
					_stateContext.TransitionTo(new PrepareShot(this));
				}
				else if (_stateContext.Current is PrepareShot)
				{
					_stateContext.TransitionTo(new Shot(this));
				}
				else
				{
					_stateContext.Clear();
					return false;
				}
			}
			else
			{
				_stateContext.Handle(entity, dstManager, ref inputData);
			}

			return true;
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
			if (_stateContext.Current == null)
			{
				_lastPosition = _target.position;
				_stateContext.TransitionTo(new Aiming(this));
			}
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
			DrawRay(_transform, Color.red, Vector3.Distance(_target.position, _transform.position));
			DrawRay(_weaponRoot, Color.yellow, Vector3.Distance(_target.position, _weaponRoot.position));
			DrawTarget(_target, Color.green);
		}

		internal float CalculateMaxAngle()
		{
			var direction = _target.position - _transform.position;
			var right = (Quaternion.AngleAxis(90, Vector3.up) * direction).normalized;
			var maxRightDirection = direction + right * SightRadius;
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

		private void DrawRay(Transform transform, Color color, float length = 40)
		{
			if (transform == null)
			{
				return;
			}

			var raySize = .1f;
			Gizmos.color = color;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(VIEW_POINT_DELTA + Vector3.forward * length / 2, new Vector3(raySize, raySize, length));
		}
	}
}