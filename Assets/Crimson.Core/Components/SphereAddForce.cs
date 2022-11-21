using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class SphereAddForce : MonoBehaviour, IActorAbility, IActorAbilityTarget
	{
		public Entity _entity;
		public EntityManager _entityManager;
		public ForceMode Mode = ForceMode.Force;
		public Vector3 Offset;
		public float Power = 25;
		public float Radius = 5;
		public float UpwardModifier = 3.0f;
		public IActor AbilityOwnerActor { get; set; }
		public IActor Actor { get; set; }
		public IActor TargetActor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			AbilityOwnerActor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Execute()
		{
			AddForceTo(TargetActor);
		}

		private void AddForceTo(IActor target)
		{
			var data = new ExplosionForceData()
			{
				Force = Power,
				ForceMode = (int)Mode,
				Position = transform.position + Offset,
				Radius = Radius,
				UpwardModifier = UpwardModifier,
			};
			_entityManager.AddComponentData(target.ActorEntity, data);
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position + Offset, Radius);
		}

#endif
	}
}