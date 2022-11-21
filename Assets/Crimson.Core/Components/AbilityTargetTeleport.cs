using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityTargetTeleport : MonoBehaviour, IActorAbility
	{
		public AbilityFindTargetActor AbilityTarget;
		public float PositionThreshold = 1;

		public List<GameObject> PreFX = new List<GameObject>();
		public List<GameObject> PostFX = new List<GameObject>();

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
			var teleportData = new TeleportData()
			{
				PositionThreshold = PositionThreshold
			};
			_entityManager.AddComponentData(_entity, teleportData);
		}

		public void SpawnFX(List<GameObject> items)
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

				ActorSpawn.Spawn(spawnData, Actor, null);
			}
		}
	}
}