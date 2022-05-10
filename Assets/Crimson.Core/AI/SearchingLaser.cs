using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.AI
{
	public class SearchingLaser
	{
		public Transform Target;
		private Quaternion _fromRotation;
		private float _rotationThreshold = .1f;
		private bool _runned;
		private float _speedTime;
		private float _startTime;
		private Quaternion _targetRotation;

		public SearchingLaser(float angle, float speedTime)
		{
			MaxAngle = angle;
			_speedTime = speedTime;
		}

		public bool IsStopped { get; private set; } = true;
		public float MaxAngle { get; set; }
		private float Angle { get; set; }

		private Quaternion TargetRotation
		{
			get => _targetRotation;
			set
			{
				var isNew = _targetRotation != value;
				_targetRotation = value;
				if (isNew)
				{
					_startTime = Time.timeSinceLevelLoad;
					IsStopped = false;
				}
			}
		}

		public void ResetAngle()
		{
			TargetRotation = Quaternion.identity;
			Angle = MaxAngle;
		}

		public void Run()
		{
			_runned = true;
			Angle = MaxAngle;
			if (IsStopped)
			{
				CalculateRotation(Angle);
			}
		}

		public void Update()
		{
			if (IsStopped)
			{
				return;
			}

			var lerpValue = (Time.timeSinceLevelLoad - _startTime) / _speedTime;
			var targetAngle = Quaternion.Angle(TargetRotation, Target.localRotation);
			if (targetAngle > _rotationThreshold)
			{
				Target.localRotation = Quaternion.Lerp(_fromRotation, TargetRotation, lerpValue);
			}
			else
			{
				if (!_runned)
				{
					Angle = math.lerp(Angle, 0, .5f);
				}
				else
				{
					_runned = false;
				}
				CalculateRotation(Angle);
			}

			if (Angle <= _rotationThreshold)
			{
				Target.localRotation = Quaternion.identity;
				IsStopped = true;
			}
		}

		private void CalculateRotation(float angle)
		{
			var randomAngle = UnityEngine.Random.Range(-180, 180) * Mathf.Deg2Rad;
			var randomAngles = new float2()
			{
				x = angle * math.cos(randomAngle),
				y = angle * math.sin(randomAngle)
			};
			_fromRotation = Target.localRotation;
			TargetRotation = Quaternion.Euler(randomAngles.x, randomAngles.y, 0);
		}
	}
}