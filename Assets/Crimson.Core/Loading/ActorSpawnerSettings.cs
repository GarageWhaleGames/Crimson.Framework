using Assets.Crimson.Core.Loading;
using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Crimson.Core.Loading
{
    [Serializable]
    public struct ActorSpawnerSettings : IActorSpawnerSettings
    {
        [ShowIf(nameof(parentOfSpawns), TargetType.ComponentName)]
        public string actorWithComponentName;

        [HideIf("@parentOfSpawns == TargetType.Spawner || parentOfSpawns == TargetType.None")]
        [EnumToggleButtons]
        public ChooseTargetStrategy chooseParentStrategy;

        public List<GameObject> copyComponentsFromSamples;

        [ShowIf("@copyComponentsFromSamples.Count > 0")]
        [InfoBox(
            "Transforms and IActors will not be copied in any case!\n Only components of chosen type will be replaced!")]
        public ComponentsOfType copyComponentsOfType;

        [ShowIf("@copyComponentsFromSamples.Count > 0")]
        public bool deleteExistingComponents;

        public bool destroyAbilityAfterSpawn;
        public FillMode fillSpawnPoints;
        [Space] public List<GameObject> objectsToSpawn;
        [EnumToggleButtons] public TargetType parentOfSpawns;

        [ShowIf(nameof(parentOfSpawns), TargetType.ChooseByTag)]
        [ValueDropdown(nameof(Tags))]
        public string parentTag;

        public ObjectPoolSettings poolSettings;

        public int randomSeed;
        public RotationOfSpawns rotationOfSpawns;
        public bool runSpawnActionsOnObjects;

        [ShowIf(nameof(spawnPosition), SpawnPosition.UseSpawnPoints)]
        public bool skipBusySpawnPoints;

        [Space] public bool spawnerDisabled;

        [ShowIf(nameof(spawnPosition), SpawnPosition.UseSpawnPoints)]
        [EnumToggleButtons]
        public FillOrder spawnPointsFillingMode;

        [ShowIf(nameof(spawnPosition), SpawnPosition.UseSpawnPoints)]
        [EnumToggleButtons]
        public SpawnPointsSource spawnPointsFrom;

        [ShowIf(nameof(spawnPosition), SpawnPosition.UseSpawnPoints)]
        [ShowIf(nameof(SpawnPointsFrom), SpawnPointsSource.FindByTag)]
        [ValueDropdown(nameof(Tags))]
        public string spawnPointTag;

        public SpawnPosition spawnPosition;

        [ShowIf(nameof(spawnPosition), SpawnPosition.UseSpawnPoints)]
        public bool useChildrenObjects;

        [ShowIf(nameof(fillSpawnPoints), FillMode.PlaceEachObjectXTimes)]
        public int x;

        private Random _rnd;

        [ShowIf(nameof(spawnPosition), SpawnPosition.UseSpawnPoints)]
        [ShowIf(nameof(SpawnPointsFrom), SpawnPointsSource.Manually)]
        [SceneObjectsOnly]
        [SerializeField]
        private List<GameObject> _spawnPoints;

        public string ActorWithComponentName
        {
            get => actorWithComponentName;
            set => actorWithComponentName = value;
        }

        public ChooseTargetStrategy ChooseStrategy
        {
            get => chooseParentStrategy;
            set => chooseParentStrategy = value;
        }

        public List<GameObject> CopyComponentsFromSamples
        {
            get
            {
                if (copyComponentsFromSamples == null)
                {
                    copyComponentsFromSamples = new List<GameObject>();
                }

                copyComponentsFromSamples.RemoveAll(go => go == null);

                return copyComponentsFromSamples;
            }
            set => copyComponentsFromSamples = value;
        }

        public ComponentsOfType CopyComponentsOfType
        {
            get => copyComponentsOfType;
            set => copyComponentsOfType = value;
        }

        public bool DeleteExistingComponents
        {
            get => deleteExistingComponents;
            set => deleteExistingComponents = value;
        }

        public bool DestroyAbilityAfterSpawn
        {
            get => !SpawnerDisabled && destroyAbilityAfterSpawn;
            set => destroyAbilityAfterSpawn = value;
        }

        public FillMode FillSpawnPoints
        {
            get => fillSpawnPoints;
            set => fillSpawnPoints = value;
        }

        public List<GameObject> ObjectsToSpawn
        {
            get => objectsToSpawn ??= new List<GameObject>();
            set => objectsToSpawn = value;
        }

        public TargetType ParentOfSpawns
        {
            get => parentOfSpawns;
            set => parentOfSpawns = value;
        }

        public string ParentTag
        {
            get => parentTag;
            set => parentTag = value;
        }

        public ObjectPoolSettings PoolSettings => poolSettings;

        public int RandomSeed
        {
            get => randomSeed;
            set => randomSeed = value;
        }

        public Random Rnd
        {
            get
            {
                if (_rnd == null)
                {
                    _rnd = randomSeed == default ? new System.Random() : new System.Random(randomSeed);
                }

                return _rnd;
            }
        }

        public RotationOfSpawns RotationOfSpawns
        {
            get => rotationOfSpawns;
            set => rotationOfSpawns = value;
        }

        public bool RunSpawnActionsOnObjects
        {
            get => runSpawnActionsOnObjects;
            set => runSpawnActionsOnObjects = value;
        }

        public bool SkipBusySpawnPoints
        {
            get => skipBusySpawnPoints;
            set => skipBusySpawnPoints = value;
        }

        public bool SpawnerDisabled
        {
            get => spawnerDisabled;
            set => spawnerDisabled = value;
        }

        public List<GameObject> SpawnPoints
        {
            get
            {
                if (_spawnPoints == null)
                {
                    _spawnPoints = new List<GameObject>();
                }

                _spawnPoints.RemoveAll(go => go == null);

                return _spawnPoints;
            }
            set => _spawnPoints = value;
        }

        public FillOrder SpawnPointsFillingMode
        {
            get => spawnPointsFillingMode;
            set => spawnPointsFillingMode = value;
        }

        public SpawnPointsSource SpawnPointsFrom
        {
            get => spawnPointsFrom;
            set => spawnPointsFrom = value;
        }

        public string SpawnPointTag
        {
            get => spawnPointTag;
            set => spawnPointTag = value;
        }

        public SpawnPosition SpawnPosition
        {
            get => spawnPosition;
            set => spawnPosition = value;
        }

        public bool UseChildrenObjects
        {
            get => useChildrenObjects;
            set => useChildrenObjects = value;
        }

        public int X
        {
            get => x;
            set => x = value;
        }

        public void InitPool()
        {
            if (!PoolSettings.UsePool)
            {
                return;
            }
            for (var i = 0; i < objectsToSpawn.Count; i++)
            {
                PoolDictionary.Instance.Register(objectsToSpawn[i], poolSettings);
            }
        }

        private static IEnumerable Tags()
        {
            return EditorUtils.GetEditorTags();
        }
    }
}