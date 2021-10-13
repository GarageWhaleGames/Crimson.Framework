using Crimson.Core.Components;
using Crimson.Core.Utils;
using UnityEngine;

namespace Crimson.Core.Common
{
    public class TimerBaseBehaviour : MonoBehaviour, ITimer
    {
        public TimerComponent Timer
        {
            get
            {
                if (this == null || this.gameObject == null) return null;
                return _timer = this.gameObject.GetOrCreateTimer(_timer);
            }
        }


        public bool TimerActive { get; set; }

        private TimerComponent _timer;

        public virtual void StartTimer()
        {
            TimerActive = true;
        }

        public virtual void FinishTimer()
        {
            TimerActive = false;
        }

        public virtual void ResetTimer()
        {
            Timer.TimedActions.Clear();
        }
    }
}