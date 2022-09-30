using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using System.Collections.Generic;
using UnityEngine;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace Assets.Plugins.Behavior_Designer.Behavior_Designer_Movement.Scripts.Tasks
{
	[TaskCategory("Movement")]
	public class FindTarget : Conditional
	{
		[Tooltip("Should a debug look ray be drawn to the scene view?")]
		public SharedBool drawDebugRay;

		[Tooltip("The LayerMask of the objects to ignore when performing the line of sight check")]
		public LayerMask ignoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");

		[Tooltip("If true, the object must be within line of sight to be within distance. For example, if this option is enabled then an object behind a wall will not be within distance even though it may " +
				 "be physically close to the other object")]
		public SharedBool lineOfSight;

		[Tooltip("The distance that the object needs to be within")]
		public SharedFloat magnitude = 5;

		[Tooltip("The LayerMask of the objects that we are searching for")]
		public LayerMask objectLayerMask;

		[Tooltip("The raycast offset relative to the pivot position")]
		public SharedVector3 offset;

		[Tooltip("The object variable that will be set when a object is found what the object is")]
		public SharedGameObject returnedObject;

		[Tooltip("The object that we are searching for")]
		public SharedGameObject targetObject;

		[Tooltip("The target raycast offset relative to the pivot position")]
		public SharedVector3 targetOffset;

		[Tooltip("The tag of the object that we are searching for")]
		public SharedString targetTag;

		[Tooltip("Should the 2D version be used?")]
		public bool usePhysics2D;

		private List<GameObject> objects;
		private bool overlapCast = false;

		// distance * distance, optimization so we don't have to take the square root
		private float sqrMagnitude;

		// Draw the seeing radius
		public override void OnDrawGizmos()
		{
#if UNITY_EDITOR
			if (Owner == null || magnitude == null)
			{
				return;
			}
			var oldColor = UnityEditor.Handles.color;
			UnityEditor.Handles.color = Color.yellow;
			UnityEditor.Handles.DrawWireDisc(Owner.transform.position, usePhysics2D ? Owner.transform.forward : Owner.transform.up, magnitude.Value);
			UnityEditor.Handles.color = oldColor;
#endif
		}

		public override void OnReset()
		{
			usePhysics2D = false;
			targetObject = null;
			targetTag = string.Empty;
			objectLayerMask = 0;
			magnitude = 5;
			lineOfSight = true;
			ignoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
			offset = Vector3.zero;
			targetOffset = Vector3.zero;
		}

		public override void OnStart()
		{
			sqrMagnitude = magnitude.Value * magnitude.Value;

			if (objects != null)
			{
				objects.Clear();
			}
			else
			{
				objects = new List<GameObject>();
			}
			// if objects is null then find all of the objects using the layer mask or tag
			if (targetObject.Value == null)
			{
				if (!string.IsNullOrEmpty(targetTag.Value))
				{
					var gameObjects = GameObject.FindGameObjectsWithTag(targetTag.Value);
					for (int i = 0; i < gameObjects.Length; ++i)
					{
						objects.Add(gameObjects[i]);
					}
				}
				else
				{
					overlapCast = true;
				}
			}
			else
			{
				objects.Add(targetObject.Value);
			}
		}

		// returns success if any object is within distance of the current object. Otherwise it will return failure
		public override TaskStatus OnUpdate()
		{
			if (transform == null || objects == null)
				return TaskStatus.Failure;

			if (overlapCast)
			{
				objects.Clear();
				if (usePhysics2D)
				{
					var colliders = Physics2D.OverlapCircleAll(transform.position, magnitude.Value, objectLayerMask.value);
					for (int i = 0; i < colliders.Length; ++i)
					{
						objects.Add(colliders[i].gameObject);
					}
				}
				else
				{
					var colliders = Physics.OverlapSphere(transform.position, magnitude.Value, objectLayerMask.value);
					for (int i = 0; i < colliders.Length; ++i)
					{
						objects.Add(colliders[i].gameObject);
					}
				}
			}

			Vector3 direction;
			// check each object. All it takes is one object to be able to return success
			for (int i = 0; i < objects.Count; ++i)
			{
				if (objects[i] == null)
				{
					continue;
				}
				direction = objects[i].transform.position - (transform.position + offset.Value);
				// check to see if the square magnitude is less than what is specified
				if (Vector3.SqrMagnitude(direction) < sqrMagnitude)
				{
					// the magnitude is less. If lineOfSight is true do one more check
					if (lineOfSight.Value)
					{
						var hitTransform = MovementUtility.LineOfSight(transform, offset.Value, objects[i], targetOffset.Value, usePhysics2D, ignoreLayerMask.value, drawDebugRay.Value);
						if (hitTransform != null)
						{
							// the object has a magnitude less than the specified magnitude and is within sight. Set the object and return success
							returnedObject.Value = objects[i];
							return TaskStatus.Success;
						}
					}
					else
					{
						// the object has a magnitude less than the specified magnitude. Set the object and return success
						returnedObject.Value = objects[i];
						return TaskStatus.Success;
					}
				}
			}
			// no objects are within distance. Return failure
			return TaskStatus.Failure;
		}
	}
}