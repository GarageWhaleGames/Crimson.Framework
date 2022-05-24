using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Forces
{
	[HideMonoScript]
	public class AbilityPushForce : MonoBehaviour, IActorAbility, IActorAbilityTarget
	{
		public EntityManager _dstManager;
		public Entity _entity;

		public Vector3 Direction;

		public float Force = 25;

		public ForceMode ForceMode;

		public IActor Actor { get; set; }
		public IActor TargetActor { get; set; }
		public IActor AbilityOwnerActor { get; set; }

		private ForceData _force => new ForceData
		{
			Force = Force,
			ForceMode = (int)ForceMode
		};

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Execute()
		{
			AddForceTo(TargetActor);
		}

		private void AddForceTo(IActor actor)
		{
			if (!_dstManager.Exists(actor.ActorEntity))
			{
				return;
			}
			var forceData = _force;
			forceData.Direction = transform.TransformDirection(Direction).normalized;
			_dstManager.AddComponent<AdditionalForceActorTag>(actor.ActorEntity);
			if (_dstManager.HasComponent<ForceData>(actor.ActorEntity))
			{
				_dstManager.SetComponentData(actor.ActorEntity, forceData);
			}
			else
			{
				_dstManager.AddComponentData(actor.ActorEntity, forceData);
			}
		}
	}
}