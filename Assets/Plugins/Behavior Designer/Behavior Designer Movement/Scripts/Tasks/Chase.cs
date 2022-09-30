using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using UnityEngine;

namespace Assets.Plugins.Behavior_Designer.Behavior_Designer_Movement.Scripts.Tasks
{
	[TaskDescription("Chase")]
	[TaskCategory("Movement")]
	public class Chase : NavMeshMovement
	{
		[UnityEngine.Tooltip("How far to predict the distance ahead of the target. Lower values indicate less distance should be predicated")]
		public SharedFloat targetDistPrediction = 20;

		[UnityEngine.Tooltip("Multiplier for predicting the look ahead distance")]
		public SharedFloat targetDistPredictionMult = 20;

		[UnityEngine.Tooltip("The GameObject that the agent is pursuing")]
		public SharedGameObject target;

		// The position of the target at the last frame
		private Vector3 targetPosition;

		public override void OnStart()
		{
			base.OnStart();
		}

		// Pursue the destination. Return success once the agent has reached the destination.
		// Return running if the agent hasn't reached the destination yet
		public override TaskStatus OnUpdate()
		{
			if (target.Value == null)
			{
				return TaskStatus.Inactive;
			}
			targetPosition = target.Value.transform.position;
			SetDestination(Target());

			return HasArrived() ? TaskStatus.Success : TaskStatus.Running;
		}

		// Predict the position of the target
		private Vector3 Target()
		{
			// Calculate the current distance to the target and the current speed
			var distance = (target.Value.transform.position - transform.position).magnitude;
			var speed = Velocity().magnitude;

			float futurePrediction = 0;
			// Set the future prediction to max prediction if the speed is too small to give an accurate prediction
			if (speed <= distance / targetDistPrediction.Value)
			{
				futurePrediction = targetDistPrediction.Value;
			}
			else
			{
				futurePrediction = (distance / speed) * targetDistPredictionMult.Value; // the prediction should be accurate enough
			}

			// Predict the future by taking the velocity of the target and multiply it by the future prediction
			var prevTargetPosition = targetPosition;
			targetPosition = target.Value.transform.position;
			return targetPosition + (targetPosition - prevTargetPosition) * futurePrediction;
		}

		// Reset the public variables
		public override void OnReset()
		{
			base.OnReset();

			targetDistPrediction = 20;
			targetDistPredictionMult = 20;
			target = null;
		}
	}
}