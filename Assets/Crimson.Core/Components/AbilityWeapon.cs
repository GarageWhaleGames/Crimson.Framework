using Assets.Crimson.Core.Components.Weapons;
using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Loading.SpawnDataTypes;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityWeapon :
		TimerBaseBehaviour,
		IActorAbility,
		IActorSpawnerAbility,
		IHasComponentName,
		IEnableable,
		ICooldownable,
		IBindable,
		IAimable,
		IHasClip
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

		[InfoBox("Force burst on single fire input. Has no effect if CoolDown Time == 0")]
		public bool forceBursts;

		[ShowIf("forceBursts")]
		[MinMaxSlider(1, 20)]
		public Vector2Int burstShotCountRange = new Vector2Int(1, 1);

		[InfoBox("Put here IEnable implementation to display reload")]
		[Space]
		public List<MonoBehaviour> reloadDisplayToggle = new List<MonoBehaviour>();

		[HideIf("projectileClipCapacity", 0f)]
		[Space]
		public List<MonoBehaviour> clipReloadDisplayToggle = new List<MonoBehaviour>();

		public ActorProjectileSpawnAnimProperties actorProjectileSpawnAnimProperties;

		public bool suppressWeaponSpawn = false;
		public List<GameObject> _aimEffects = new List<GameObject>();

		public readonly List<GameObject> _aimEffectInstances = new List<GameObject>();

		[HideInInspector] public List<string> appliedPerksNames = new List<string>();

		[HideInInspector]
		public bool aimingByInput;

		public List<GameObject> SpawnedObjects { get; private set; }
		public List<Action<GameObject>> SpawnCallbacks { get; set; }
		public Action<GameObject> DisposableSpawnCallback { get; set; }
		public bool IsEnable { get; set; }

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

		public WeaponClip ClipData { get; } = new WeaponClip();

		private EntityManager _dstManager;

		private bool _circlePrefabScaled;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;

			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_entity = entity;

			ClipData.Setup(projectileClipCapacity, projectileClipCapacity);

			SpawnCallbacks = new List<Action<GameObject>>();

			IsEnable = true;

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

			if (!Actor.Abilities.Contains(this)) Actor.Abilities.Add(this);

			SpawnPointsRoot = new GameObject("spawn points root").transform;
			SpawnPointsRoot.SetParent(gameObject.transform);

			SpawnPointsRoot.localPosition = Vector3.zero;
			for (var i = 0; i < _aimEffects.Count; i++)
			{
				var item = _aimEffects[i];
				var spawnData = new SpawnItemData()
				{
					Owner = actor,
					Spawner = actor,
					Prefab = item
				};
				var instance = ActorSpawn.Spawn(spawnData);
				instance.transform.SetParent(SpawnPointsRoot, false);
				_aimEffectInstances.Add(instance);
			}
			ResetSpawnPointRootRotation();

			if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnerPosition)
			{
				projectileSpawnData.SpawnPosition = SpawnPosition.UseSpawnPoints;
				projectileSpawnData.spawnPointsFrom = SpawnPointsSource.Manually;
				projectileSpawnData.rotationOfSpawns = RotationOfSpawns.UseSpawnPointRotation;
			}

			if (projectileSpawnData.SpawnPoints.Any())
			{
				projectileSpawnData.SpawnPoints = new List<GameObject>();
			}

			var baseSpawnPoint = new GameObject("Base Spawn Point");
			baseSpawnPoint.transform.SetParent(SpawnPointsRoot);

			baseSpawnPoint.transform.localPosition = Vector3.zero;
			baseSpawnPoint.transform.localRotation = Quaternion.identity;

			projectileSpawnData.SpawnPoints.Add(baseSpawnPoint);
		}

		internal void DestroyAimEffects()
		{
			if (_aimEffectInstances.Count == 0)
			{
				return;
			}
			for (var i = 0; i < _aimEffectInstances.Count; i++)
			{
				Destroy(_aimEffectInstances[i]);
			}
			_aimEffectInstances.Clear();
		}

		public void Execute()
		{
			if (!IsEnable)
			{
				return;
			}

			// ReSharper disable once CompareOfFloatsByEqualityOperator Here we need exact comparison
			if (projectileStartupDelay == 0 &&
				World.DefaultGameObjectInjectionWorld.EntityManager.Exists(_entity))
			{
				findTargetProperties.SearchCompleted = false;
				Spawn();

				World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(_entity,
					new ActorProjectileThrowAnimTag());

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (CooldownTime == 0) return;

				StartTimer();

				if (!forceBursts)
				{
					Timer.TimedActions.AddAction(FinishTimer, CooldownTime);
				}
				else
				{
					var _additionalShotsNum = Random.Range(burstShotCountRange.x, burstShotCountRange.y) - 1;
					for (var i = 1; i <= _additionalShotsNum; i++)
					{
						Timer.TimedActions.AddAction(Spawn, CooldownTime * i);
					}
					Timer.TimedActions.AddAction(FinishTimer, CooldownTime * (_additionalShotsNum + 1));
				}
				if (projectileClipCapacity == 0)
				{
					return;
				}

				ClipData.Decrease();
				if (ClipData.IsEmpty)
				{
					Timer.TimedActions.AddAction(Reload, clipReloadTime);
				}
			}
			else if (Timer != null)
			{
				Timer.TimedActions.AddAction(Spawn, projectileStartupDelay);
			}
		}

		public void EvaluateAim(Vector2 pos)
		{
			aimingByInput = true;
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
			aimingByInput = false;

			this.ResetAiming(Actor);
			_circlePrefabScaled = false;

			if (AimingProperties.evaluateActionOptions != EvaluateActionOptions.RepeatingEvaluation) return;
			OnHoldAttackActive = false;
			ResetSpawnPointRootRotation();
		}

		public void ResetSpawnPointRootRotation()
		{
			if (SpawnPointsRoot == null) return;

			if (attackDirectionType == AttackDirectionType.Forward)
			{
				SpawnPointsRoot.localRotation = Quaternion.identity;
				return;
			}

			SpawnPointsRoot.localRotation = Quaternion.Euler(0, -180, 0);
		}

		public void SetAimEffectsState(bool state)
		{
			for (var i = 0; i < _aimEffectInstances.Count; i++)
			{
				var instance = _aimEffectInstances[i];
				instance.SetActive(state);
			}
		}

		public void Spawn()
		{
			if (!suppressWeaponSpawn)
			{
				if (onClickAttackType == OnClickAttackType.AutoAim && !OnHoldAttackActive && !findTargetProperties.SearchCompleted)
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

			//if (!_actorToUi) return;

			ResetSpawnPointRootRotation();
			OnHoldAttackActive = false;
			findTargetProperties.SearchCompleted = false;
		}

		public void Reload()
		{
			ClipData.Reload();
		}

		public override void FinishTimer()
		{
			base.FinishTimer();
			IsEnable = true;

			this.FinishAbilityCooldownTimer(Actor);
		}

		public override void StartTimer()
		{
			base.StartTimer();
			IsEnable = false;

			this.StartAbilityCooldownTimer(Actor);
		}

		public void EvaluateAimByArea(Vector2 pos)
		{
			var lastSpawnedAimingPrefabPos = AbilityUtils.EvaluateAimByArea(this, pos * -1);

			Actor.GameObject.GetComponent<Rigidbody>().rotation =
				Quaternion.Euler(0, -180, 0) * SpawnedAimingPrefab.transform.rotation;

			if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnPoints)
			{
				SpawnPointsRoot.rotation =
					Quaternion.Euler(0, -180, 0) * SpawnedAimingPrefab.transform.rotation;
			}

			DisposableSpawnCallback = go =>
			{
				var targetActor = go.GetComponent<Actor>();
				if (targetActor == null)
				{
					return;
				}

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

		public void InitPool()
		{
			projectileSpawnData.InitPool();
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

	public struct ActorProjectileThrowAnimTag : IComponentData
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