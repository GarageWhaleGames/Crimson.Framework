using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityForceMovement : MonoBehaviour, IActorAbility
	{
		public IActor Actor { get; set; }

		[InfoBox("To set movement speed and dynamics, use AbilityMovement component")]
		[EnumToggleButtons] public MoveDirection moveDirection;

		[ShowIf(nameof(moveDirection), MoveDirection.UseDirection)]
		public Vector3 forwardVector;

		[ShowIf(nameof(moveDirection), MoveDirection.UseDirection)]
		public bool compensateSpawnerRotation = true;

		[ShowIf(nameof(moveDirection), MoveDirection.SpawnerForward)]
		public Vector3 OffsetDirection = Vector3.zero;

		public bool _useVariance;

		[ShowIf(nameof(_useVariance))]
		public TransformVariance _transformVariance;

		public Transform Spawner => Actor.Spawner.GameObject.transform;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;

			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			dstManager.AddComponentData(entity, new MoveByInputData());
			dstManager.AddComponentData(entity, new ActorForceMovementData
			{
				MoveDirection = moveDirection,
				ForwardVector = forwardVector,
				Variance = _transformVariance,
				UseVariance = _useVariance,
				OffsetDirection = OffsetDirection,
				CompensateSpawnerRotation = compensateSpawnerRotation,
				stopGuiding = false
			});
		}

		public void Execute()
		{
		}
	}

	public struct ActorForceMovementData : IComponentData
	{
		public MoveDirection MoveDirection;
		public float3 ForwardVector;
		public bool CompensateSpawnerRotation;
		public bool stopGuiding;
		public float3 OffsetDirection;
		public TransformVariance Variance;
		public bool UseVariance;
	}

	public enum MoveDirection
	{
		SpawnerForward = 0,
		UseDirection = 1,
		GuidedBySpawner = 2,
		SelfForward = 3,
		SpawnerEnemy = 4
	}
}