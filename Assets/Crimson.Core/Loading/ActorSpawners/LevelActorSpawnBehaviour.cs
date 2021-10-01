using Crimson.Core.Common;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Crimson.Core.Loading.ActorSpawners
{
    [HideMonoScript]
    public sealed class LevelActorSpawnBehaviour : MonoBehaviour, IActorSpawner, IComponentName
    {
        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        [Space] [SerializeField] public string componentName = "";
        [Space]

        public ActorSpawnerSettings SpawnData;

        public List<GameObject> SpawnedObjects { get; private set; } = new List<GameObject>();

        public void Spawn()
        {
            var spawnItems = ActorSpawn.GenerateData(SpawnData);
            SpawnedObjects.Clear();
            for (var i = 0; i < spawnItems.Count; i++)
            {
                SpawnedObjects.Add(ActorSpawn.Spawn(spawnItems[i]));
            }
        }
    }
}