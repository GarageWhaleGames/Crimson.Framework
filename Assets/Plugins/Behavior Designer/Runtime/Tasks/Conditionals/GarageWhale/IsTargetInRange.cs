using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class IsTargetInRange : Conditional
	{
		public SharedVector2 Range;

		public SharedGameObject Target;

		public override void OnDrawGizmos()
		{
			Handles.color = Color.yellow;
			Handles.DrawWireDisc(transform.position, transform.up, Range.Value.x, Range.Value.y);
		}

		public override TaskStatus OnUpdate()
		{
			if (Target.Value == null)
			{
				return TaskStatus.Failure;
			}
			var distance = Vector3.Distance(Target.Value.transform.position, transform.position);

			return distance >= Range.Value.x && distance <= Range.Value.y ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}