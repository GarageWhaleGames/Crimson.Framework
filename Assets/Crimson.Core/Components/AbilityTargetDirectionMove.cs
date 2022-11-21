using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityTargetDirectionMove : TimerBaseBehaviour, IActorAbility
	{
		public AbilityFindTargetActor AbilityTarget;

		[TitleGroup("Params", Order = 1)] public float Duration = 1;
		[TitleGroup("Effects", Order = 2), PropertyOrder(1)] public List<GameObject> PostFX = new List<GameObject>();
		[TitleGroup("Effects", Order = 2), PropertyOrder(0)] public List<GameObject> PreFX = new List<GameObject>();
		[TitleGroup("Params", Order = 1)] public float Velocity = 1;

		private Entity _entity;
		private EntityManager _entityManager;
		public IActor Actor { get; set; }
		public List<GameObject> PostFXInstance { get; set; }
		public List<GameObject> PreFXInstance { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		}

		public void DestroyFX(List<GameObject> items)
		{
			for (var i = 0; i < items.Count; i++)
			{
				GameObjectUtils.Destroy(items[i]);
			}
		}

		public void Execute()
		{
			var target = AbilityTarget.Target.transform;
			var direction = target.position - transform.position;
			var data = new DirectMoveData()
			{
				Direction = direction,
				Duration = Duration,
				Velocity = Velocity
			};
			_entityManager.AddComponentData(_entity, data);
		}

		public List<GameObject> SpawnFX(List<GameObject> items)
		{
			if (items != null && items.Count > 0)
			{
				var spawnData = new ActorSpawnerSettings
				{
					objectsToSpawn = items,
					SpawnPosition = SpawnPosition.UseSpawnerPosition,
					RotationOfSpawns = RotationOfSpawns.UseSpawnPointRotation,
					parentOfSpawns = TargetType.None,
					runSpawnActionsOnObjects = true,
					destroyAbilityAfterSpawn = true
				};

				return ActorSpawn.Spawn(spawnData, Actor, null);
			}
			else
			{
				return new List<GameObject>();
			}
		}
	}
}