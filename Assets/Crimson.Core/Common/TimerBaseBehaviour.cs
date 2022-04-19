using Crimson.Core.Components;
using Crimson.Core.Utils;
using UnityEngine;

namespace Crimson.Core.Common
{
	public class TimerBaseBehaviour : MonoBehaviour, ITimer
	{
		private TimerComponent _timer;

		public TimerComponent Timer
		{
			get
			{
				_timer = this == null || gameObject == null ? null : this.gameObject.GetOrCreateTimer(_timer);
				return _timer;
			}
		}

		public bool TimerActive { get; set; }

		public virtual void FinishTimer()
		{
			TimerActive = false;
		}

		public virtual void ResetTimer()
		{
			Timer.TimedActions.Clear();
		}

		public virtual void StartTimer()
		{
			TimerActive = true;
		}
	}
}