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
	public class AbilityTargetDash : TimerBaseBehaviour, IActorAbility
	{
		public AbilityFindTargetActor AbilityTarget;
		public float PositionThreshold = 1;

		[ValueDropdown(nameof(_modes))]
		public string Mode;

		public float Value = 2;

		public List<GameObject> PreFX = new List<GameObject>();
		public List<GameObject> PostFX = new List<GameObject>();

		private Entity _entity;
		private EntityManager _entityManager;
		public IActor Actor { get; set; }

		private string[] _modes = new string[]
			{
				"Duration",
				"Velocity"
			};

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		}

		public void Execute()
		{
			var target = AbilityTarget.Target.transform;
			var direction = target.position - transform.position;
			var ray = new Ray(transform.position, direction);
			var results = new RaycastHit[1];
			var positionThreshold = PositionThreshold;
			if (Physics.RaycastNonAlloc(ray, results, direction.magnitude) > 0)
			{
				return;
			}
			var lookToTarget = Quaternion.LookRotation(direction);
			transform.rotation = lookToTarget;
			SpawnFX(PreFX);
			var moveData = new MoveData()
			{
				EndPosition = target.position,
				PositionThreshold = PositionThreshold
			};

			float duration = 0;
			switch (Mode)
			{
				case "Duration":
					moveData.Velocity = direction.magnitude / Value;
					duration = Value;
					break;

				case "Velocity":
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