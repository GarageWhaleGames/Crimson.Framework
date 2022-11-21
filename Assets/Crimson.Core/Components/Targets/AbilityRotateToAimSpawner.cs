using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Targets
{
	public class AbilityRotateToAimSpawner : MonoBehaviour, IActorAbility
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
			var target = Actor;
			while (target != null)
			{
				target = TryLookAtAim(target) ? null : target.Spawner;
			}
		}

		private bool TryLookAtAim(IActor source)
		{
			if (!_entityManager.HasComponent<AimData>(source.ActorEntity))
			{
				return false;
			}
			var enemy = _entityManager.GetComponentData<EnemyTargetData>(source.ActorEntity);
			if (enemy.Entity != Entity.Null)
			{
				transform.LookAt(enemy.Position);
			}
			return true;
		}
	}
}