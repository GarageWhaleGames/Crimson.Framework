using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class CanSeeTarget : Conditional
	{
		public SharedVector3 BoxSize;
		public Color GizmoColor = Color.green;
		public Color GizmoHitColor = Color.red;
		public LayerMask Layer;
		public SharedFloat MaxDistance = 10;
		public SharedInt MaxResults = 20;
		public SharedVector3 Offset;
		public SharedGameObject Result;
		public SharedGameObjectList Results;
		private RaycastHit[] _results;

		public override void OnAwake()
		{
			_results = new RaycastHit[MaxResults.Value];
		}

		public override void OnDrawGizmos()
		{
			Gizmos.color = Result.Value != null ? GizmoHitColor : GizmoColor;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Offset.Value, BoxSize.Value);
		}

		public override TaskStatus OnUpdate()
		{
			Results.Value = new List<GameObject>(MaxResults.Value);
			var boxSize = BoxSize.Value;
			var offset = Offset.Value;
			var size = Physics.BoxCastNonAlloc(
				transform.TransformPoint(offset),
				boxSize / 2,
				transform.forward,
				_results,
				transform.rotation,
				0,
				Layer);
			for (var i = 0; i < size; i++)
			{
				if (i == 0)
				{
					Result.Value = _results[0].transform.gameObject;
				}
				Results.Value.Add(_results[i].transform.gameObject);
			}
			return size > 0 ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}