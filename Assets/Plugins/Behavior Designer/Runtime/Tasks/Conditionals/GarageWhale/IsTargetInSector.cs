using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class IsTargetInSector : Conditional
	{
		public SharedVector2 Sector = new Vector2(-180, 180);
		public SharedGameObject Target;
		private Vector3 _initDirection;

		public override void OnAwake()
		{
			_initDirection = transform.forward;
		}

		public override TaskStatus OnUpdate()
		{
			if (Target == null || Target.Value == null)
			{
				return TaskStatus.Failure;
			}
			var target = Target.Value.transform;
			var sector = Sector.Value;
			var angle = Vector3.SignedAngle(_initDirection, target.position - transform.position, transform.up);
			return angle >= sector.x && angle <= sector.y ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}