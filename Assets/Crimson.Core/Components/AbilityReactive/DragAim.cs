using Crimson.Core.Common;
using Crimson.Core.Components.Interfaces;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components.AbilityReactive
{
    public class DragAim : TimerBaseBehaviour,
        IActorAbility,
        IComponentName,
        IEnableable,
        IAimable,
        IDragable
    {
        public AimingAnimationProperties aimingAnimProperties;
        public bool aimingAvailable;

        [Space]
        [ShowInInspector]
        public string componentName = "";

        public bool deactivateAimingOnCooldown;

        public AimingProperties _aimingProperties;

        [SerializeField, InfoBox("Time when aim disable (Seconds)")]
        private float _resetAimTime = 0.15f;

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

        public AimingProperties AimingProperties { get => _aimingProperties; set => _aimingProperties = value; }
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
        public bool Enabled { get; set; }
        public bool OnHoldAttackActive { get; set; }
        public List<Action<GameObject>> SpawnCallbacks { get; set; }
        public GameObject SpawnedAimingPrefab { get; set; }
        public List<GameObject> SpawnedObjects { get; private set; } = new List<GameObject>();
        protected Entity CurrentEntity { get; private set; }
        protected EntityManager CurrentEntityManager { get; private set; }

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;

            CurrentEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            CurrentEntity = entity;

            SpawnCallbacks = new List<Action<GameObject>>();

            Enabled = true;

            CurrentEntityManager.AddComponent<TimerData>(entity);

            if (AimingAnimProperties.HasActorAimingAnimation)
            {
                CurrentEntityManager.AddComponentData(entity, new AimingAnimProperties
                {
                    AnimHash = Animator.StringToHash(AimingAnimProperties.ActorAimingAnimationName)
                });
            }
        }

        public void BeginDrag(float2 input)
        {
        }

        public void Drag(float2 input)
        {
            EvaluateAim(input);
        }

        public void EndDrag(float2 input)
        {
            ResetAiming();
        }

        public void EvaluateAim(Vector2 pos)
        {
            this.EvaluateAim(Actor as Actor, pos);
        }

        public void EvaluateAimByArea(Vector2 pos)
        {
            var aimPosition = AbilityUtils.EvaluateAimByArea(this, pos);

            DisposableSpawnCallback = go =>
            {
                var targetActor = go.GetComponent<Actor>();
                if (targetActor == null)
                {
                    return;
                }

                var vector = Quaternion.Euler(0, -180, 0) * aimPosition;
            };
        }

        public void EvaluateAimBySelectedType(Vector2 pos)
        {
            EvaluateAimTarget(pos);
        }

        public void EvaluateAimBySight(Vector2 pos)
        {
            var aimPosition = this.EvaluateAimBySight(Actor, pos);

            DisposableSpawnCallback = go =>
            {
                var targetActor = go.GetComponent<Actor>();
                if (targetActor == null)
                {
                    return;
                }

                var vector = aimPosition - Actor.GameObject.transform.position;

                CurrentEntityManager.AddComponentData(targetActor.ActorEntity, new DestroyProjectileInPointData
                {
                    Point = new float2(aimPosition.x, aimPosition.z)
                });
            };
        }

        public void EvaluateAimTarget(Vector2 input)
        {
            switch (AimingProperties.aimingType)
            {
                case AimingType.AimingArea:
                    EvaluateAimByArea(input);
                    break;

                case AimingType.SightControl:
                    EvaluateAimBySight(input);
                    break;

            }
        }

        public void Execute()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator Here we need exact comparison
            if (Enabled && CurrentEntityManager.Exists(CurrentEntity))
            {
                StartTimer();
                this.RestartAction(FinishTimer, _resetAimTime);
            }
        }

        public override void FinishTimer()
        {
            base.FinishTimer();
            Enabled = true;
        }

        public void ResetAiming()
        {
            this.ResetAiming(Actor);
            if (AimingProperties.evaluateActionOptions != EvaluateActionOptions.RepeatingEvaluation)
            {
                return;
            }
            OnHoldAttackActive = false;
        }
        public override void StartTimer()
        {
            base.StartTimer();
            Enabled = false;
        }
    }
}