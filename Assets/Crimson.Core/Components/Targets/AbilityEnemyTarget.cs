using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Targets
{
	public class AbilityEnemyTarget : MonoBehaviour, IActorAbility
	{
		public FindTargetProperties FindTargetProperties;

		private Entity _entity;
		private EntityManager _entityManager;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_entityManager.AddComponentData(_entity, new EnemyTargetData());
			_entityManager.AddComponentData(_entity, new FindEnemyTargetTag());
		}

		public void Execute()
		{
		}
	}
}