using Crimson.Core.Common;
using Crimson.Core.Enums;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityFollowMovement : MonoBehaviour, IActorAbility
	{
		private Entity _entity;

		public IActor Actor { get; set; }

		private EntityManager _dstManager;
		public bool RunOnAwake = true;

		public FindTargetProperties findTargetProperties;

		[Space][EnumToggleButtons] public FollowType followMovementType;

		[ShowIf("followMovementType", FollowType.Simple)]
		[InfoBox("Speed of 0 results in unconstrained speed")]
		[MinValue(0)]
		public float movementSpeed = 0;

		[ShowIf("followMovementType", FollowType.Simple)]
		public bool retainOffset = false;

		[ShowIf("followMovementType", FollowType.UseMovementComponent)]
		public bool continousFollow = true;

		public bool hideIfNoTarget = false;

		public Transform Target
		{
			get => _target;
			set => _target = value;
		}

		private Transform _target;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (RunOnAwake)
			{
				Run(entity);
			}
		}

		private void Run(Entity entity)
		{
			_dstManager.AddComponentData(entity, new ActorFollowMovementData());
			_dstManager.AddComponentData(entity, new ActorNoFollowTargetMovementData());
			if (followMovementType == FollowType.UseMovementComponent)
			{
				_dstManager.AddComponentData(entity, new MoveByInputData());
			}
			else
			{
				_dstManager.AddComponentData(entity, new MoveDirectlyData
				{
					Speed = movementSpeed
				});
			}
		}

		public void Execute()
		{
			Run(_entity);
		}
	}

	public struct ActorFollowMovementData : IComponentData
	{
		public float3 Origin;
	}

	public struct ActorNoFollowTargetMovementData : IComponentData
	{
	}
}