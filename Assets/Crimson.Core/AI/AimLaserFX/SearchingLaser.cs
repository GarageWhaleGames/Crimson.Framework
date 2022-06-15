using UnityEngine;

namespace Assets.Crimson.Core.AI.AimLaserFX
{
	public class SearchingLaser
	{
		public Transform Target;
		private float _speedTime;
		private ILaserState _state;
		private LaserStateType _stateType;

		public SearchingLaser(float angle, float speedTime)
		{
			Angle = angle;
			_speedTime = speedTime;
		}

		public float Angle { get; set; }
		public bool IsStopped { get; private set; } = true;

		public LaserStateType StateType
		{
			get => _stateType;
			set
			{
				var isNew = !_stateType.Equals(value);
				if (isNew)
				{
					_stateType = value;
					UpdateState();
				}
			}
		}

		private void UpdateState()
		{
			_state?.Stop();
			switch (StateType)
			{
				case LaserStateType.Idle:
					_state = new IdleState();
					break;

				case LaserStateType.Searching:
					_state = new SearchingState(Target, Angle, _speedTime);
					break;

				case LaserStateType.Aim:
					_state = new AimState(Target, _speedTime);
					break;

				default:
					break;
			}
			_state.Run();
		}
	}
}