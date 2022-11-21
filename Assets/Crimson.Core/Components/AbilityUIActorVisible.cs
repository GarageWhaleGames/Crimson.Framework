using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityUIActorVisible : MonoBehaviour, IActorAbility
	{
		public Canvas Target;
		private Entity _entity;
		private EntityManager _entityManager;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		}

		public void Execute()
		{
			Target.enabled = !_entityManager.HasComponent<InvisibleTag>(Actor.Spawner.ActorEntity);
		}
	}
}