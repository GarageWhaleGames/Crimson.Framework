using UnityEngine;

namespace Assets.Crimson.Core.AI.Behaviours
{
	public class DelayTimer
	{
		private float _delayTime;
		private float _startTime = default;

		public float ElapsedTime => Time.realtimeSinceStartup - _startTime;

		public bool IsExpire => ElapsedTime > _delayTime;

		public bool IsStopped => _startTime == default;

		public void Start(float delay)
		{
			_delayTime = delay;
			_startTime = Time.realtimeSinceStartup;
		}

		public void Stop()
		{
			_startTime = default;
		}
	}
}