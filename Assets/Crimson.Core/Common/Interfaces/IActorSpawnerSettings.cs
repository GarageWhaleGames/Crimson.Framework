using Assets.Crimson.Core.Loading;
using Crimson.Core.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Crimson.Core.Common
{
	public interface IActorSpawnerSettings
	{
		string ActorWithComponentName { get; set; }
		ChooseTargetStrategy ChooseStrategy { get; set; }
		List<GameObject> CopyComponentsFromSamples { get; set; }
		ComponentsOfType CopyComponentsOfType { get; set; }
		bool DeleteExistingComponents { get; set; }
		FillMode FillSpawnPoints { get; set; }
		List<GameObject> ObjectsToSpawn { get; set; }
		TargetType ParentOfSpawns { get; set; }
		string ParentTag { get; set; }
		ObjectPoolSettings PoolSettings { get; }
		int RandomSeed { get; set; }
		System.Random Rnd { get; }
		RotationOfSpawns RotationOfSpawns { get; set; }
		bool RunSpawnActionsOnObjects { get; set; }
		bool SkipBusySpawnPoints { get; set; }
		bool SpawnerDisabled { get; set; }
		List<GameObject> SpawnPoints { get; set; }
		FillOrder SpawnPointsFillingMode { get; set; }
		SpawnPointsSource SpawnPointsFrom { get; set; }
		string SpawnPointTag { get; set; }
		SpawnPosition SpawnPosition { get; set; }
		bool UseChildrenObjects { get; set; }
		int X { get; set; }

		void InitPool();
	}
}