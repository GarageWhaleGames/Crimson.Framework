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
	public class AbilityActorMove : TimerBaseBehaviour, IActorAbility
	{
		public bool ExecuteOnAwake;

		[TitleGroup("Move params")] public bool IgnoreRaycast;

		[EnumToggleButtons, TitleGroup("Move params")]
		public MoveByType Mode;

		public Vector3 MoveVector;
		public List<GameObject> PostFX = new List<GameObject>();
		public List<GameObject> PreFX = new List<GameObject>();
		[TitleGroup("Move params")] public float Value;
		private Entity _entity;
		private EntityManager _entityManager;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (ExecuteOnAwake)
			{
				Execute();
			}
		}

		[Button]
		public void Execute()
		{
			SpawnFX(PreFX);
			var targetPosition = transform.position + MoveVector;
			var direction = MoveVector;
			var moveData = new MoveData()
			{
				EndPosition = targetPosition,
				IgnoreRaycast = IgnoreRaycast
			};

			float duration = 0;
			switch (Mode)
			{
				case MoveByType.Time:
					moveData.Velocity = direction.magnitude / Value;
					duration = Value;
					break;

				case MoveByType.Velocity:
					moveData.Velocity = Value;
					duration = direction.magnitude / Value;
					break;

				default:
					break;
			}
			_entityManager.AddComponentData(_entity, moveData);
			this.AddAction(() =>
			{
				SpawnFX(PostFX);
			}, duration);
		}

		private void SpawnFX(List<GameObject> items)
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