using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityApplyInvisible : MonoBehaviour, IActorAbility
	{
		private Entity _entity;
		private EntityManager _entityManager;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			Actor = actor;
		}

		public void Execute()
		{
			_entityManager.AddComponentData(_entity, new InvisibleTag());
		}
	}
}