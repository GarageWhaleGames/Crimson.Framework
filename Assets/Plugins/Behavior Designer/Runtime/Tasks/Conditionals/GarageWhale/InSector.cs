using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class InSector : Conditional
	{
		public SharedVector2 Sector = new Vector2(-180, 180);
		private Vector3 _initDirection;

		public override void OnAwake()
		{
			_initDirection = transform.forward;
		}

		public override TaskStatus OnUpdate()
		{
			var sector = Sector.Value;
			var angle = Vector3.SignedAngle(_initDirection, transform.forward, transform.up);
			return angle >= sector.x && angle <= sector.y ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}