using Crimson.Core.Common;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityMovement : MonoBehaviour, IActorAbility
	{
		public IActor Actor { get; set; }

		[MinValue(0)] public float movementSpeed = 1f;

		public bool useDynamics = true;

		[ShowIf("@useDynamics == true")] public MovementDynamics movementStart;
		[ShowIf("@useDynamics == true")] public MovementDynamics movementEnd;

		public MovementAnimationProperies movementAnimationProperties;

		private MovementDynamicsInner _dynamics;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_dynamics.useDynamics = useDynamics;
			if (useDynamics)
			{
				_dynamics.curveIn.keyframe0 = movementStart.curve.keys[0];
				_dynamics.curveIn.keyframe1 = movementStart.curve.keys[movementStart.curve.keys.Length - 1];
				_dynamics.curveOut.keyframe0 = movementEnd.curve.keys[0];
				_dynamics.curveOut.keyframe1 = movementEnd.curve.keys[movementEnd.curve.keys.Length - 1];
				_dynamics.timeScaleIn = movementStart.timeScale;
				_dynamics.timeScaleOut = movementEnd.timeScale;
			}

			dstManager.AddComponentData(entity, new ActorMovementData
			{
				MovementSpeed = movementSpeed,
				Dynamics = _dynamics,
				InRatio = 0f,
				OutRatio = 1f,
				ExternalMultiplier = 1f
			});

			if (movementAnimationProperties.HasMovementAnimation)
			{
				dstManager.AddComponentData(entity, new ActorMovementAnimationData
				{
					AnimHash = Animator.StringToHash(movementAnimationProperties.MovementAnimationName),
					SpeedFactorHash =
						Animator.StringToHash(movementAnimationProperties.MovementAnimationSpeedFactorName),
					SpeedFactorMultiplier = movementAnimationProperties.MovementAnimationSpeedFactorMultiplier
				});
			}
		}

		public void Execute()
		{
		}
	}

	public struct ActorMovementData : IComponentData
	{
		public float MovementSpeed;
		public MovementDynamicsInner Dynamics;
		public float CurveInStartTime;
		public float CurveOutStartTime;
		public float InRatio;
		public float OutRatio;
		public float ExternalMultiplier;
		public float3 MovementCache;
		public float3 Input;
	}

	public struct ActorMovementAnimationData : IComponentData
	{
		public int AnimHash;
		public int SpeedFactorHash;
		public float SpeedFactorMultiplier;
	}
}