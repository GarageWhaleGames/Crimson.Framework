using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Common
{
    public struct GameStateData : IComponentData
    {
        public byte foo;
    }

    public class GameState : Actor
    {
        [ValueDropdown("Tags")]
        public string enemyTag;

        public GameObject losePanel;
        public int maxDeathCount = 1;

        [HideInInspector]
        public IYandexAppMetrica metrica;

        [HideInInspector]
        public List<AbilityActorPlayer> players;

        public GameObject respawnPanel;
        public GameObject rootCanvas;

        [HideInInspector]
        public ActorSpawnerSettings sampleSpawner = new ActorSpawnerSettings
        {
            ObjectsToSpawn = null,
            SpawnPosition = SpawnPosition.UseSpawnerPosition,
            SpawnPointsFillingMode = FillOrder.SequentialOrder,
            FillSpawnPoints = FillMode.UseEachObjectOnce,
            SkipBusySpawnPoints = false,
            SpawnPoints = new List<GameObject>(),
            RotationOfSpawns = RotationOfSpawns.UseZeroRotation,
            ParentOfSpawns = TargetType.None,
            chooseParentStrategy = ChooseTargetStrategy.Nearest,
            RunSpawnActionsOnObjects = true,
            DestroyAbilityAfterSpawn = false,
            CopyComponentsFromSamples = new List<GameObject>(),
            CopyComponentsOfType = ComponentsOfType.AllComponents
        };

        [HideInInspector]
        public double startTime;

        [HideInInspector]
        public AbilityActorPlayer userPlayer;

        [ValidateInput("MustBeSpawner", "Spawner MonoBehaviours must derive from IActorSpawner!!")]
        public MonoBehaviour userSpawner;

        public GameObject winPanel;

        public bool NeedCheckEndGame;

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            
            sampleSpawner.SpawnPoints = new List<GameObject> { this.gameObject };
            sampleSpawner.objectsToSpawn = new List<GameObject>
            {
                respawnPanel,
                winPanel,
                losePanel
            };
            metrica = AppMetrica.Instance;
            Setup();
            
        }
        public override void PostConvert()
        {
            WorldEntityManager.AddComponentData(ActorEntity, new GameStateData());
        }

        private static IEnumerable Tags()
        {
            return EditorUtils.GetEditorTags();
        }

        private bool MustBeSpawner(MonoBehaviour a)
        {
            return (a is IActorSpawner) || (a is null);
        }
    }
}