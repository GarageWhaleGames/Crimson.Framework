using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Targets
{
	public class AbilityRotateToEnemySpawner : MonoBehaviour, IActorAbility
	{
		public bool ExecuteOnAwake;

		private EntityManager _entityManager;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (ExecuteOnAwake)
			{
				Execute();
			}
		}

		public void Execute()
		{
			if (!_entityManager.HasComponent<EnemyTargetData>(Actor.Spawner.ActorEntity))
			{
				return;
			}
			var enemy = _entityManager.GetComponentData<EnemyTargetData>(Actor.Spawner.ActorEntity);
			if (enemy.Entity != Entity.Null)
			{
				transform.LookAt(enemy.Position);
			}
		}
	}
}