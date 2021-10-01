using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components
{
    [HideMonoScript]
    public class AbilityActorSimpleSpawn : MonoBehaviour, IActorAbility, IComponentName
    {
        [Space]
        [SerializeField]
        public string componentName = "";

        public GameObject objectToSpawn;

        [EnumToggleButtons] public SpawnerType spawnerType;

        [EnumToggleButtons] public OwnerType ownerType;

        [Space] public bool ExecuteOnAwake = false;
        [Space] public bool DestroyAfterSpawn = false;

        [HideInInspector] public GameObject spawnedObject;

        public IActor Actor { get; set; }

        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        private IActor _currentSpawner;
        private IActor _currentOwner;
        
        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;
            if (ExecuteOnAwake) Execute();
        }

        public void Execute()
        {
            _currentSpawner = spawnerType == SpawnerType.CurrentActor
                ? Actor
                : null;

            _currentOwner = ownerType == OwnerType.CurrentActorOwner
                ? Actor.Owner
                : ownerType == OwnerType.CurrentActor
                    ? Actor
                    : null;

            var spawnData = new ActorSpawnerSettings
            {
                objectsToSpawn = new List<GameObject> { objectToSpawn },
                SpawnPosition = SpawnPosition.UseSpawnerPosition,
                parentOfSpawns = TargetType.None,
                runSpawnActionsOnObjects = true,
                destroyAbilityAfterSpawn = true
            };

            var spawnItems = ActorSpawn.GenerateData(spawnData, _currentSpawner, _currentOwner);
            spawnedObject = ActorSpawn.Spawn(spawnItems[0]);

            if (DestroyAfterSpawn) Destroy(this);
        }
    }

    public enum SpawnerType
    {
        CurrentActor = 0,
        Null = 1
    }

    public enum OwnerType
    {
        CurrentActorOwner = 0,
        CurrentActor = 1,
        Null = 2
    }
}