using Crimson.Core.Common;
using Crimson.Core.Components.Interfaces;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components.AbilityReactive
{
    [HideMonoScript]
    public class WeaponSpawnOnes : TimerBaseBehaviour,
        IActorAbility,
        IActorSpawnerAbility,
        IComponentName,
        IEnableable,
        ICooldownable,
        IBindable,
        IUseAimable
    {
        public ActorProjectileSpawnAnimProperties actorProjectileSpawnAnimProperties;

        public IAimable Aim => AimComponent as IAimable;

        [HideInInspector] public List<string> appliedPerksNames = new List<string>();

        [HideIf("projectileClipCapacity", 0f)]
        [Space]
        public List<MonoBehaviour> clipReloadDisplayToggle = new List<MonoBehaviour>();

        [HideIf("projectileClipCapacity", 0f)] public float clipReloadTime = 1f;
        public string componentName = "";

        [InfoBox("Clip Capacity of 0 stands for unlimited clip")]
        public int projectileClipCapacity = 0;

        public ActorSpawnerSettings projectileSpawnData;

        [InfoBox("Put here IEnable implementation to display reload")]
        [Space]
        public List<MonoBehaviour> reloadDisplayToggle = new List<MonoBehaviour>();

        public Transform SpawnPoint;
        public bool suppressWeaponSpawn = false;
        protected Entity _entity;
        private bool _actorToUi;
        [SerializeField] private float _cooldownTime = 0.3f;
        private EntityManager _dstManager;

        [ValidateInput("MustBeAimable", "Ability MonoBehaviours must derive from IAimable!")]
        public MonoBehaviour AimComponent;

        public bool ActionExecutionAllowed { get; set; }
        public IActor Actor { get; set; }

        public int BindingIndex { get; set; } = -1;

        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        public float CooldownTime
        {
            get => _cooldownTime;
            set => _cooldownTime = value;
        }

        public Action<GameObject> DisposableSpawnCallback { get; set; }
        public bool Enabled { get; set; }
        public bool OnHoldAttackActive { get; set; }
        public List<Action<GameObject>> SpawnCallbacks { get; set; }
        public GameObject SpawnedAimingPrefab { get; set; }
        public List<GameObject> SpawnedObjects { get; private set; }
        public Transform SpawnPointsRoot { get; private set; }
        protected EntityManager CurrentEntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;

            _dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _entity = entity;

            SpawnCallbacks = new List<Action<GameObject>>();

            Enabled = true;

            _dstManager.AddComponent<TimerData>(entity);

            if (actorProjectileSpawnAnimProperties != null
                && actorProjectileSpawnAnimProperties.HasActorProjectileAnimation)
            {
                _dstManager.AddComponentData(entity, new ActorProjectileAnimData
                {
                    AnimHash = Animator.StringToHash(actorProjectileSpawnAnimProperties.ActorProjectileAnimationName)
                });
            }

            appliedPerksNames = new List<string>();

            var playerActor = actor.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;
            _actorToUi = playerActor != null && playerActor.actorToUI;

            if (!Actor.Abilities.Contains(this)) Actor.Abilities.Add(this);

            if (!_actorToUi) return;

            SpawnPointsRoot = new GameObject("spawn points root").transform;
            SpawnPointsRoot.SetParent(gameObject.transform);

            SpawnPointsRoot.localPosition = Vector3.zero;
            ResetSpawnPointRootRotation();

            if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnerPosition)
            {
                projectileSpawnData.SpawnPosition = SpawnPosition.UseSpawnPoints;
            }

            if (projectileSpawnData.SpawnPoints.Any()) projectileSpawnData.SpawnPoints.Clear();

            var baseSpawnPoint = new GameObject("Base Spawn Point");
            baseSpawnPoint.transform.SetParent(SpawnPointsRoot);

            baseSpawnPoint.transform.localPosition = Vector3.zero;
            baseSpawnPoint.transform.localRotation = Quaternion.identity;

            projectileSpawnData.SpawnPoints.Add(baseSpawnPoint);
        }

        public void Execute()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator Here we need exact comparison
            if (Enabled && CurrentEntityManager.Exists(_entity))
            {
                Debug.Log("WeaponSpawnOnes");
                Spawn();

                CurrentEntityManager.AddComponentData(_entity,
                    new ActorProjectileThrowAnimData());

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (CooldownTime == 0) return;

                StartTimer();
                Timer.TimedActions.AddAction(FinishTimer, CooldownTime);
            }
        }

        public override void FinishTimer()
        {
            base.FinishTimer();
            Enabled = true;

            this.FinishAbilityCooldownTimer(Actor);
        }

        public void ResetSpawnPointRootRotation()
        {
            SpawnPointsRoot.localRotation = Quaternion.identity;
        }

        public void Spawn()
        {
            if (Aim != null && Aim.SpawnedAimingPrefab != null)
            {
                SpawnPointsRoot.LookAt(Aim.SpawnedAimingPrefab.transform);
            }
            SpawnedObjects = ActorSpawn.Spawn(projectileSpawnData, Actor, Actor.Owner);
            if (SpawnedObjects == null)
            {
                return;
            }

            var objectsToSpawn = SpawnedObjects;

            if (SpawnedObjects == null)
            {
                return;
            }

            foreach (var callback in SpawnCallbacks)
            {
                objectsToSpawn.ForEach(go => callback.Invoke(go));
            }

            objectsToSpawn.ForEach(go =>
            {
                DisposableSpawnCallback?.Invoke(go);
                DisposableSpawnCallback = null;
            });

            if (!_actorToUi) return;

            ResetSpawnPointRootRotation();
            OnHoldAttackActive = false;
        }

        public override void StartTimer()
        {
            Enabled = false;
            base.StartTimer();

            this.StartAbilityCooldownTimer(Actor);
        }

        private bool MustBeAimable(MonoBehaviour behaviour)
        {
            return behaviour is IActorAbility;
        }
    }
}