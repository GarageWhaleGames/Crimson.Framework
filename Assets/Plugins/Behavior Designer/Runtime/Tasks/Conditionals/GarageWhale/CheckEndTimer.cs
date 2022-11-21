using Assets.Plugins.Behavior_Designer.Runtime.Tasks.Actions.GarageWhale;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class CheckEndTimer : Conditional
	{
		public StartTimer Timer;

		public override TaskStatus OnUpdate()
		{
			if (!Timer.IsStarted)
			{
				return TaskStatus.Failure;
			}
			var isExpired = Time.realtimeSinceStartup - Timer.StartTime >= Timer.Duration;
			if (isExpired)
			{
				Timer.IsStarted = false;
			}
			return isExpired ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}