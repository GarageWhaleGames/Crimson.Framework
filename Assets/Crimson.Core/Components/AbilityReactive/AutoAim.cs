using Crimson.Core.Common;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components.AbilityReactive
{
    public class AutoAim : TimerBaseBehaviour,
        IActorAbility,
        IActorSpawnerAbility,
        IComponentName,
        IAimable
    {
        public AimingAnimationProperties aimingAnimProperties;
        public bool aimingAvailable;

        [Space,
        ShowInInspector]
        public string componentName = "";

        public bool deactivateAimingOnCooldown;
        public FindTargetProperties FindTargetProperties = new FindTargetProperties();
        public ActorSpawnerSettings MarkSpawnData;

        [InfoBox("Time when aim disable (Seconds)")]
        public float ResetAimTime = 0.15f;

        public bool ActionExecutionAllowed { get; set; }
        public IActor Actor { get; set; }

        public AimingAnimationProperties AimingAnimProperties
        {
            get => aimingAnimProperties;
            set => aimingAnimProperties = value;
        }

        public bool AimingAvailable
        {
            get => aimingAvailable;
            set => aimingAvailable = value;
        }

        public virtual AimingProperties AimingProperties { get; set; }
        public int BindingIndex { get; set; } = -1;

        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        public bool DeactivateAimingOnCooldown
        {
            get => deactivateAimingOnCooldown;
            set => deactivateAimingOnCooldown = value;
        }

        public Action<GameObject> DisposableSpawnCallback { get; set; }
        public List<Action<GameObject>> SpawnCallbacks { get; set; }
        public GameObject SpawnedAimingPrefab { get; set; }
        public List<GameObject> SpawnedObjects { get; private set; } = new List<GameObject>();
        public Transform SpawnPointsRoot { get; private set; }
        public bool OnHoldAttackActive { get; set; }
        protected Entity CurrentEntity { get; private set; }
        protected EntityManager CurrentEntityManager { get; private set; }

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;

            CurrentEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            CurrentEntity = entity;

            SpawnCallbacks = new List<Action<GameObject>>();

            CurrentEntityManager.AddComponent<TimerData>(entity);

            if (MarkSpawnData.SpawnPoints != null
                && MarkSpawnData.SpawnPoints.Count != 0)
            {
                MarkSpawnData.SpawnPoints.Clear();
            }
        }

        public void EvaluateAim(Vector2 pos)
        {
            this.EvaluateAim(Actor as Actor, pos);
        }

        public void EvaluateAimBySelectedType(Vector2 pos)
        {
            EvaluateAimTarget(pos);
        }

        public void EvaluateAimTarget(Vector2 input)
        {
        }

        public void Execute()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator Here we need exact comparison
            if (CurrentEntityManager.Exists(CurrentEntity))
            {
                Spawn();

                StartTimer();
                this.RestartAction(FinishTimer, ResetAimTime);
            }
        }

        public override void FinishTimer()
        {
            base.FinishTimer();
            ResetAiming();
        }

        public virtual void ResetAiming()
        {
            this.ResetAiming(Actor);
            for (var i = 0; i < SpawnedObjects.Count; i++)
            {
                Destroy(SpawnedObjects[i]);
            }
            SpawnedObjects.Clear();
            SpawnedAimingPrefab = null;
        }

        public void SetTarget(Vector3 position)
        {
            SpawnedObjects.ForEach(s => { if (s != null) s.transform.position = position; });
        }

        public void Spawn()
        {
            SpawnMarkIfNull();
            if (!FindTargetProperties.SearchCompleted)
            {
                CurrentEntityManager.AddComponentData(CurrentEntity, new AutoAimTargetData());
                return;
            }

            FindTargetProperties.SearchCompleted = false;
        }

        private void SpawnMarkIfNull()
        {
            if (SpawnedObjects == null || SpawnedObjects.Count == 0)
            {
                SpawnedObjects = ActorSpawn.Spawn(MarkSpawnData, Actor, Actor);
                if (SpawnedObjects.Count > 0)
                {
                    SpawnedAimingPrefab = SpawnedObjects[0];
                }
            }
        }
    }
}