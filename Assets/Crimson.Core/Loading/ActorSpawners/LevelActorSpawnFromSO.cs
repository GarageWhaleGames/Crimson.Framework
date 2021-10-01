using Crimson.Core.Common;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Crimson.Core.Loading.ActorSpawners
{
    [HideMonoScript]
    public class LevelActorSpawnFromSO : MonoBehaviour, IActorSpawner, IComponentName
    {
        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        [Space] [SerializeField] public string componentName = "";
        [Space]

        public LevelActorSpawnerDataSO spawnDataFile;

        public List<GameObject> SpawnedObjects { get; private set; }

        public void Spawn()
        {
            Assert.IsNotNull(spawnDataFile);

            var spawnItems = ActorSpawn.GenerateData(spawnDataFile.SpawnData);
            SpawnedObjects.Clear();
            for (var i = 0; i < spawnItems.Count; i++)
            {
                SpawnedObjects.Add(ActorSpawn.Spawn(spawnItems[i]));
            }
        }
    }
}