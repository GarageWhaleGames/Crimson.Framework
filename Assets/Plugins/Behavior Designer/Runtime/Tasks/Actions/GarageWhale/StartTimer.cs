using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Actions.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class StartTimer : Action
	{
		public SharedVector2 DurationRange;

		[ReadOnly] public float StartTime;
		public bool IsStarted;

		public float Duration { get; private set; }

		public override void OnStart()
		{
			IsStarted = true;
			Duration = Random.Range(DurationRange.Value.x, DurationRange.Value.y);
			StartTime = Time.realtimeSinceStartup;
		}
	}
}