using System.Collections.Generic;
using Crimson.Core.Enums;
using UnityEngine;

namespace Crimson.Core.Common
{
    public interface IActorSpawnerSettings
    {
        bool SpawnerDisabled { get; set; }
        List<GameObject> ObjectsToSpawn { get; set; }
        SpawnPosition SpawnPosition { get; set; }
        FillOrder SpawnPointsFillingMode { get; set; }
        FillMode FillSpawnPoints { get; set; }
        int X { get; set; }
        bool SkipBusySpawnPoints { get; set; }
        List<GameObject> SpawnPoints { get; set; }
        SpawnPointsSource SpawnPointsFrom { get; set; }
        string SpawnPointTag { get; set; }
        RotationOfSpawns RotationOfSpawns { get; set; }
        TargetType ParentOfSpawns { get; set; }
        string ActorWithComponentName { get; set; }
        string ParentTag { get; set; }
        ChooseTargetStrategy ChooseStrategy { get; set; }
        bool RunSpawnActionsOnObjects { get; set; }
        int RandomSeed { get; set; }
        System.Random Rnd { get; }
        List<GameObject> CopyComponentsFromSamples { get; set; }
        ComponentsOfType CopyComponentsOfType { get; set; }
        bool DeleteExistingComponents { get; set; }
    }
}