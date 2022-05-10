using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.AI
{
	public class AIPathControl
	{
		private Transform _parent;
		private NavMeshAgent _navAgent;

		public Vector3 EndWaypointPosition => _navAgent.pathEndPosition;

		public bool IsValid => _navAgent.pathStatus != NavMeshPathStatus.PathInvalid;

		public bool HasArrived => _navAgent.remainingDistance <= _navAgent.stoppingDistance;

		public Vector3 NextPosition => _navAgent.nextPosition;

		public float2 MoveDirection
		{
			get
			{
				if (HasArrived)
				{
					return float2.zero;
				}
				var direction = math.normalizesafe(new float2(_navAgent.velocity.x, _navAgent.velocity.z), float2.zero);
				return direction;
			}
		}

		public void SetTarget(Transform target)
		{
			SetTarget(target.position);
		}

		public void SetTarget(Vector3 target)
		{
			_navAgent.SetDestination(target);
			_navAgent.nextPosition = _parent.position;
		}

		public AIPathControl(Transform parent)
		{
			_parent = parent;
			_navAgent = parent.GetComponent<NavMeshAgent>();
			_navAgent.updatePosition = false;
			_navAgent.updateRotation = false;
		}
	}
}