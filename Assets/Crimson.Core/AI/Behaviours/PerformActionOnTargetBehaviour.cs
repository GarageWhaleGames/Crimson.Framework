using Assets.Crimson.Core.AI.GeneralParams;
using Assets.Crimson.Core.AI.Interfaces;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.AI
{
	[HideMonoScript]
	public class PerformActionOnTargetBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour, IDrawGizmos
	{
		public BasePriority Priority = new BasePriority
		{
			Value = 1
		};

		public CurvePriority CurvePriority = new CurvePriority(0)
		{
			XAxisTooltip = "Target priority based on distance to it"
		};

		public EvaluationMode EvaluationMode;

		public TagFilter TagFilter;
		public ExecutableCustomInput CustomInput;
		public DistanceLimitation DistanceLimitation;

		public IActor Actor { get; set; }

		private const float FINISH_CHASE_DISTSQ = 5f;
		private const float PRIORITY_MULTIPLIER = 0.5f;
		private readonly Vector3 VIEW_POINT_DELTA = new Vector3(0f, 0.6f, 0f);

		private Transform _target = null;
		private Transform _transform = null;
		private readonly AIPathControl _path = new AIPathControl(finishThreshold: FINISH_CHASE_DISTSQ);

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			_target = null;
			_transform = Actor.GameObject.transform;

			if (_transform == null)
			{
				return 0f;
			}

			var filteredTargets = targets.Where(t => TagFilter.Filter(t) && t != _transform).ToList();
			if (filteredTargets.Count == 0)
			{
				return 0f;
			}

			if (filteredTargets.Count == 1)
			{
				_target = filteredTargets.First();
				return math.distancesq(_transform.position, _target.position) < FINISH_CHASE_DISTSQ ? 0f :
					Priority.Value * PRIORITY_MULTIPLIER;
			}

			switch (EvaluationMode)
			{
				case EvaluationMode.Random:
					var priorities = new List<MinMaxTarget>();

					var priorityCache = 0f;

					foreach (var target in filteredTargets)
					{
						var distnace = math.distance(_transform.position, target.position);
						var priority = CurvePriority.Evaluate(distnace);

						priorities.Add(new MinMaxTarget
						{
							Min = priorityCache,
							Max = priority + priorityCache,
							Target = target
						});

						priorityCache += priority;
					}

					var randomNumber = UnityEngine.Random.Range(0f, priorityCache);

					_target = priorities.Find(t => t.Min < randomNumber && t.Max >= randomNumber).Target;
					break;

				default: // ReSharper disable once RedundantCaseLabel
				case EvaluationMode.Strict:
					var orderedTargets = filteredTargets.OrderBy(t =>
					{
						var distance = math.distance(_transform.position, t.position);
						return CurvePriority.Evaluate(distance);
					}).ToList();

					_target = orderedTargets.Last();
					break;
			}

			return math.distancesq(_transform.position, _target.position) < FINISH_CHASE_DISTSQ ? 0f :
				Priority.Value * PRIORITY_MULTIPLIER;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			return _path.Setup(_transform, _target);
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			if (!_path.IsValid)
			{
				return false;
			}

			_path.NextPoint();

			if (Physics.Raycast(_transform.position + VIEW_POINT_DELTA, _target.position - _transform.position, out var hit,
				DistanceLimitation.MaxDistance) && hit.transform == _target)
			{
				inputData.Move = float2.zero;
				inputData.CustomInput[CustomInput.CustomInputIndex] = 1f;
				return true;
			}

			if (_path.HasArrived)
			{
				inputData.Move = float2.zero;
				inputData.CustomInput[CustomInput.CustomInputIndex] = 1f;

				return false;
			}

			var dir = _path.Direction;

			inputData.Move = math.normalize(new float2(dir.x, dir.z));

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