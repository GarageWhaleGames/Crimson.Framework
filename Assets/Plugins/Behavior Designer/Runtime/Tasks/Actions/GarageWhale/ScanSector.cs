using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Actions.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class ScanSector : Action
	{
		public SharedFloat AngleSpeed = 20;

		[MinMaxSlider(-180, 180, true)]
		public SharedVector2 Sector = new Vector2(-180, 180);

		private Vector3 _initDirection;
		private bool _invertAngle;

		public override void OnAwake()
		{
			_initDirection = transform.forward;
		}

		public override TaskStatus OnUpdate()
		{
			var sector = Sector.Value;
			var velocity = AngleSpeed.Value * Time.deltaTime;
			if (_invertAngle)
			{
				velocity *= -1;
			}
			var angle = Vector3.SignedAngle(_initDirection, transform.forward, transform.up);
			var targetAngle = angle + velocity;
			var clampedAngle = Mathf.Clamp(targetAngle, sector.x, sector.y);
			var currentAngle = velocity - (targetAngle - clampedAngle);
			transform.rotation *= Quaternion.AngleAxis(currentAngle, transform.up);
			var isMinimalLimit = clampedAngle <= sector.x;
			var isMaximumLimit = clampedAngle >= sector.y;
			if (isMinimalLimit || isMaximumLimit)
			{
				_invertAngle = !_invertAngle;
			}

			return TaskStatus.Success;
		}

		private void OnDrawGizmosSelected()
		{
			Handles.color = Color.magenta;
			Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, Sector.Value.x, 2);
			Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, Sector.Value.y, 2);
		}
	}
}