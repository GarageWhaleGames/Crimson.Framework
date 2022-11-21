using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Actions.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class RandomDirectionMove : NavMeshMovement
	{
		public SharedFloat Distance = 1;
		public Color GizmosColor = Color.green;
		public SharedVector2 Range;
		public SharedGameObject Target;

		private bool _hasTarget;
		private int _maxPoints = 16;
		private Vector3 _target;

		public override void OnDrawGizmos()
		{
			Gizmos.color = GizmosColor;
			var positions = CalculatePositionsOnCircle(transform.position, Distance.Value, _maxPoints);
			for (var i = 0; i < positions.Length; i++)
			{
				var position = positions[i];
				Gizmos.DrawWireSphere(position, .4f);
			}
		}

		public override void OnStart()
		{
			base.OnStart();
		}

		// Pursue the destination. Return success once the agent has reached the destination.
		// Return running if the agent hasn't reached the destination yet
		public override TaskStatus OnUpdate()
		{
			if (Distance.Value == 0)
			{
				return TaskStatus.Inactive;
			}

			if (!_hasTarget)
			{
				_hasTarget = true;
				_target = GetRandomLocation(Distance.Value, _maxPoints);
				SetDestination(_target);
			}

			transform.LookAt(Target.Value.transform);

			if (HasArrived())
			{
				_hasTarget = false;
				return TaskStatus.Success;
			}
			else
			{
				return TaskStatus.Running;
			};
		}

		private Vector3[] CalculatePositionsOnCircle(Vector3 sourcePosition, float distance, int points)
		{
			var angleRad = Mathf.Sin(Mathf.PI / points);
			var smallRadius = angleRad * distance;
			var angleDegree = 360 / points;
			var initPosition = Vector3.forward * distance;
			var results = new Vector3[points];
			for (var i = 0; i < points; i++)
			{
				var position = initPosition + sourcePosition;
				results[i] = NavMesh.SamplePosition(position, out var hit, smallRadius, NavMesh.AllAreas)
					? hit.position
					: position;
				initPosition = Quaternion.AngleAxis(angleDegree, Vector3.up) * initPosition;
			}
			return results;
		}

		private Vector3 GetRandomLocation(float distanceLimit, int maxPoints)
		{
			var positions = CalculatePositionsOnCircle(transform.position, distanceLimit, maxPoints);
			if (TryGetRandomReachable(navMeshAgent, positions, out var result))
			{
				return result;
			}

			return transform.position;
		}

		private bool InRange(Vector3 postion, Vector3 target, Vector2 range)
		{
			var distance = Vector3.Distance(target, postion);
			return distance >= range.x && distance <= range.y;
		}

		private bool TryGetRandomReachable(NavMeshAgent agent, Vector3[] positions, out Vector3 resultPosition)
		{
			var result = false;
			resultPosition = transform.position;

			var randomPositions = positions
				.Where(s => InRange(s, Target.Value.transform.position, Range.Value))
				.OrderBy(s => Random.value);
			var path = new NavMeshPath();
			foreach (var position in randomPositions)
			{
				if (agent.CalculatePath(position, path))
				{
					result = true;
					resultPosition = position;
				}
			}

			return result;
		}
	}
}