using Crimson.Core.Common;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.AI
{
	public class AIPathControl
	{
		private int _currentWaypoint;
		private float _finishThreshold;
		private Transform _parent;
		private NavMeshPath _path;
		private float _threshold;
		public Vector3 Direction => math.normalize(Path.corners[_currentWaypoint] - _parent.position);

		public Vector3 EndWaypointPosition => IsValid ? Path.corners[Path.corners.Length - 1] : Vector3.zero;

		public bool HasArrived
		{
			get
			{
				if (_currentWaypoint != _path.corners.Length - 1)
				{
					return _currentWaypoint >= Path.corners.Length;
				}
				else
				{
					var distance = math.distancesq(_parent.position, Path.corners[_currentWaypoint]);
					return distance <= _finishThreshold;
				}
			}
		}

		public bool IsValid => _parent != null && Path.status != NavMeshPathStatus.PathInvalid;

		public NavMeshPath Path
		{
			get
			{
				if (_path == null)
				{
					_path = new NavMeshPath();
				}
				return _path;
			}
		}

		public bool CalculatePath(Vector3 position)
		{
			_currentWaypoint = 0;
			return NavMesh.CalculatePath(_parent.position, position, NavMesh.AllAreas, Path);
		}

		public bool NextPoint()
		{
			var result = false;

			if (_currentWaypoint < Path.corners.Length)
			{
				var distSq = math.distancesq(_parent.position, Path.corners[_currentWaypoint]);
				if (distSq <= _threshold)
				{
					_currentWaypoint++;
					result = true;
				}
			}

			return result;
		}

		public AIPathControl(float waypointThreshold = Constants.WAYPOINT_SQDIST_THRESH, float finishThreshold = 0.1f)
		{
			_threshold = waypointThreshold;
			_finishThreshold = finishThreshold;
		}

		internal bool Setup(Transform transform, Transform target)
		{
			return target != null && Setup(transform, target.position);
		}

		internal bool Setup(Transform transform, Vector3 position)
		{
			if (transform == null)
			{
				return false;
			}
			_parent = transform;

			return CalculatePath(position);
		}
	}
}