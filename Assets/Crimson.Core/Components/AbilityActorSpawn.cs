using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Loading;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityActorSpawn : TimerBaseBehaviour, IActorAbility, IActorSpawnerAbility, IHasComponentName
	{
		[Space]
		[SerializeField]
		public string componentName = "";

		[Space] public bool ExecuteNow = false;
		[Space] public bool ExecuteOnAwake = false;
		[Space] public ActorSpawnerSettings SpawnData;
		[Space] public TimerDelays SpawnDelays;
		private readonly SpawnTimerCollection _spawnedObjectCollection = new SpawnTimerCollection();
		public IActor Actor { get; set; }

		public string ComponentName
		{
			get => componentName;
			set => componentName = value;
		}

		public Action<GameObject> DisposableSpawnCallback { get; set; }
		public List<Action<GameObject>> SpawnCallbacks { get; set; } = new List<Action<GameObject>>();
		public List<GameObject> SpawnedObjects => _spawnedObjectCollection;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			InitPool();
			_spawnedObjectCollection.Clear();
			Actor = actor;
			World.DefaultGameObjectInjectionWorld.EntityManager.AddComponent<TimerData>(entity);
			if (ExecuteOnAwake) Execute();
		}

		[Button]
		public void Execute()
		{
			Spawn();
			DestroyAbilityAfterSpawn();
		}

		public void InitPool()
		{
			SpawnData.InitPool();
		}

		public void Spawn()
		{
			if (SpawnDelays.StartupDelay == 0 && ExecuteNow)
			{
				_spawnedObjectCollection.AddRange(ActorSpawn.Spawn(SpawnData, Actor, Actor.Owner));
			}
			else
			{
				_spawnedObjectCollection.SetItems(ActorSpawn.GenerateData(SpawnData, Actor, Actor?.Owner));
				_spawnedObjectCollection.SpawnWithOptions(Timer, SpawnDelays);
			}
		}

		private void DestroyAbilityAfterSpawn()
		{
			if (SpawnData.DestroyAbilityAfterSpawn) Destroy(this);
		}
	}
}