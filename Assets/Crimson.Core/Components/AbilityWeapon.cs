using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
    [HideMonoScript]
    public class AbilityWeapon : TimerBaseBehaviour, IActorAbility, IActorSpawnerAbility, IComponentName, IEnableable,
        ICooldownable, IBindable, IAimable
    {
        public IActor Actor { get; set; }

        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        [Space]
        [ShowInInspector]
        [SerializeField]
        public string componentName = "";

        public bool primaryProjectile;

        public bool aimingAvailable;
        public bool deactivateAimingOnCooldown;

        [EnumToggleButtons] public OnClickAttackType onClickAttackType = OnClickAttackType.DirectAttack;

        [EnumToggleButtons] public AttackDirectionType attackDirectionType = AttackDirectionType.Forward;

        [ShowIf("onClickAttackType", OnClickAttackType.AutoAim)]
        public FindTargetProperties findTargetProperties;

        public AimingProperties aimingProperties;
        public AimingAnimationProperties aimingAnimProperties;

        [Space] public ActorSpawnerSettings projectileSpawnData;

        //TODO: Consider making this class child of AbilityActorSpawn, and leave all common fields to parent

        [Space] public float projectileStartupDelay = 0f;

        public float cooldownTime = 0.3f;

        [InfoBox("Clip Capacity of 0 stands for unlimited clip")]
        public int projectileClipCapacity = 0;

        [HideIf("projectileClipCapacity", 0f)] public float clipReloadTime = 1f;

        [InfoBox("Put here IEnable implementation to display reload")]
        [Space]
        public List<MonoBehaviour> reloadDisplayToggle = new List<MonoBehaviour>();

        [HideIf("projectileClipCapacity", 0f)]
        [Space]
        public List<MonoBehaviour> clipReloadDisplayToggle = new List<MonoBehaviour>();

        public ActorProjectileSpawnAnimProperties actorProjectileSpawnAnimProperties;

        public bool suppressWeaponSpawn = false;

        [HideInInspector] public List<string> appliedPerksNames = new List<string>();
        public List<GameObject> SpawnedObjects { get; private set; }
        public List<Action<GameObject>> SpawnCallbacks { get; set; }
        public Action<GameObject> DisposableSpawnCallback { get; set; }
        public bool Enabled { get; set; }

        public float CooldownTime
        {
            get => cooldownTime;
            set => cooldownTime = value;
        }

        public int BindingIndex { get; set; } = -1;

        public Transform SpawnPointsRoot { get; private set; }

        public bool AimingAvailable
        {
            get => aimingAvailable;
            set => aimingAvailable = value;
        }

        public bool DeactivateAimingOnCooldown
        {
            get => deactivateAimingOnCooldown;
            set => deactivateAimingOnCooldown = value;
        }

        public bool ActionExecutionAllowed { get; set; }
        public GameObject SpawnedAimingPrefab { get; set; }

        public AimingProperties AimingProperties
        {
            get => aimingProperties;
            set => aimingProperties = value;
        }

        public AimingAnimationProperties AimingAnimProperties
        {
            get => aimingAnimProperties;
            set => aimingAnimProperties = value;
        }

        public bool OnHoldAttackActive { get; set; }

        private Entity _entity;
        private EntityManager _dstManager;
        private int _projectileClip;
        private bool _actorToUi;

        private bool _circlePrefabScaled;

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;

            _dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _entity = entity;

            _projectileClip = projectileClipCapacity;

            SpawnCallbacks = new List<Action<GameObject>>();

            Enabled = true;

            _dstManager.AddComponent<TimerData>(entity);

            if (actorProjectileSpawnAnimProperties.HasActorProjectileAnimation)
            {
                _dstManager.AddComponentData(entity, new ActorProjectileAnimData
                {
                    AnimHash = Animator.StringToHash(actorProjectileSpawnAnimProperties.ActorProjectileAnimationName)
                });
            }

            if (AimingAnimProperties.HasActorAimingAnimation)
            {
                _dstManager.AddComponentData(entity, new AimingAnimProperties
                {
                    AnimHash = Animator.StringToHash(AimingAnimProperties.ActorAimingAnimationName)
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
            if (Enabled && projectileStartupDelay == 0 &&
                World.DefaultGameObjectInjectionWorld.EntityManager.Exists(_entity))
            {
                Spawn();

                World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(_entity,
                    new ActorProjectileThrowAnimData());

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (CooldownTime == 0) return;

                StartTimer();
                Timer.TimedActions.AddAction(FinishTimer, CooldownTime);

                if (projectileClipCapacity == 0) return;

                _projectileClip--;
                if (_projectileClip < 1)
                {
                    Timer.TimedActions.AddAction(Reload, clipReloadTime);
                }
            }
            else if (Enabled && Timer != null)
            {
                Timer.TimedActions.AddAction(Spawn, projectileStartupDelay);
            }
        }

        public void EvaluateAim(Vector2 pos)
        {
            this.EvaluateAim(Actor as Actor, pos);
        }

        public void EvaluateAimBySelectedType(Vector2 pos)
        {
            switch (AimingProperties.aimingType)
            {
                case AimingType.AimingArea:
                    EvaluateAimByArea(pos);
                    break;
                case AimingType.SightControl:
                    EvaluateAimBySight(pos);
                    break;
                case AimingType.Circle:
                    EvaluateAimByCircle();
                    break;
            }
        }

        public void EvaluateAimByCircle()
        {
            if (_circlePrefabScaled) return;

            var objectsToSpawn = projectileSpawnData.ObjectsToSpawn;
            if (!objectsToSpawn.Any() || objectsToSpawn.Count > 1) return;

            var objectToSpawn = objectsToSpawn.First();

            var abilityCollision = objectToSpawn.GetComponent<AbilityCollision>();

            if (abilityCollision == null) return;

            var coll = abilityCollision.useCollider;

            if (coll == null) return;

            var colliderRadius = 0f;

            switch (coll)
            {
                case SphereCollider sphere:
                    colliderRadius = sphere.radius;
                    break;
                case CapsuleCollider capsule:
                    var direction = capsule.direction;

                    colliderRadius = (direction == 0 || direction == 2)
                        ? capsule.height * 0.5f
                        : capsule.radius;
                    break;
                case BoxCollider box:
                    var size = box.size;
                    colliderRadius = Math.Max(size.x, size.z) * 0.5f;
                    break;
            }

            SpawnedAimingPrefab.transform.localScale *= colliderRadius;

            _circlePrefabScaled = true;
        }

        public void ResetAiming()
        {
            this.ResetAiming(Actor);
            _circlePrefabScaled = false;

            if (AimingProperties.evaluateActionOptions != EvaluateActionOptions.RepeatingEvaluation) return;
            OnHoldAttackActive = false;
            ResetSpawnPointRootRotation();
        }

        public void ResetSpawnPointRootRotation()
        {
            if (attackDirectionType == AttackDirectionType.Forward)
            {
                SpawnPointsRoot.localRotation = Quaternion.identity;
                return;
            }

            SpawnPointsRoot.localRotation = Quaternion.Euler(0, -180, 0);
        }

        public void Spawn()
        {
            if (!suppressWeaponSpawn)
            {
                if (onClickAttackType == OnClickAttackType.AutoAim && !OnHoldAttackActive &&
                    _actorToUi && !findTargetProperties.SearchCompleted)
                {
                    _dstManager.AddComponentData(_entity, new FindAutoAimTargetData
                    {
                        WeaponComponentName = ComponentName
                    });

                    return;
                }

                SpawnedObjects = ActorSpawn.Spawn(projectileSpawnData, Actor, Actor.Owner);
            }

            var objectsToSpawn = suppressWeaponSpawn
                ? projectileSpawnData.ObjectsToSpawn
                : SpawnedObjects;

            if (objectsToSpawn == null) return;

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
            findTargetProperties.SearchCompleted = false;
        }

        public void Reload()
        {
            _projectileClip = projectileClipCapacity;
        }

        public override void FinishTimer()
        {
            base.FinishTimer();
            Enabled = true;

            this.FinishAbilityCooldownTimer(Actor);
        }

        public override void StartTimer()
        {
            base.StartTimer();
            Enabled = false;

            this.StartAbilityCooldownTimer(Actor);
        }

        public void EvaluateAimByArea(Vector2 pos)
        {
            var lastSpawnedAimingPrefabPos = AbilityUtils.EvaluateAimByArea(this, pos);

            if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnPoints)
            {
                SpawnPointsRoot.rotation =
                    Quaternion.Euler(0, -180, 0) * SpawnedAimingPrefab.transform.rotation;
            }

            DisposableSpawnCallback = go =>
            {
                var targetActor = go.GetComponent<Actor>();
                if (targetActor == null) return;

                var vector = Quaternion.Euler(0, -180, 0) * lastSpawnedAimingPrefabPos;

                targetActor.ChangeActorForceMovementData(
                    projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnPoints ? go.transform.forward : vector);
            };
        }

        public void EvaluateAimBySight(Vector2 pos)
        {
            var lastSpawnedAimingPrefabPos = this.EvaluateAimBySight(Actor, pos);

            if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnPoints)
            {
                SpawnPointsRoot.LookAt(SpawnedAimingPrefab.transform);
            }

            DisposableSpawnCallback = go =>
            {
                var targetActor = go.GetComponent<Actor>();
                if (targetActor == null) return;

                var vector = lastSpawnedAimingPrefabPos - Actor.GameObject.transform.position;

                targetActor.ChangeActorForceMovementData(
                    projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnPoints ? go.transform.forward : vector);

                _dstManager.AddComponentData(targetActor.ActorEntity, new DestroyProjectileInPointData
                {
                    Point = new float2(lastSpawnedAimingPrefabPos.x,
                        lastSpawnedAimingPrefabPos.z)
                });
            };
        }
    }

    public enum OnClickAttackType
    {
        DirectAttack = 0,
        AutoAim = 1
    }

    public enum AttackDirectionType
    {
        Forward = 0,
        Backward = 1
    }


    public enum AimingType
    {
        AimingArea = 0,
        SightControl = 1,
        Circle = 2
    }

    public enum EvaluateActionOptions
    {
        EvaluateOnce = 0,
        RepeatingEvaluation = 1
    }

    public struct DestroyProjectileInPointData : IComponentData
    {
        public float2 Point;
    }

    public struct ActorProjectileAnimData : IComponentData
    {
        public int AnimHash;
    }

    public struct ActorProjectileThrowAnimData : IComponentData
    {
    }

    public struct BindedActionsCooldownData : IComponentData
    {
        public FixedList32<int> ReadyToUseBindingIndexes;
        public FixedList32<int> OnCooldownBindingIndexes;
    }

    public struct FindAutoAimTargetData : IComponentData
    {
        public FixedString128 WeaponComponentName;
    }
}