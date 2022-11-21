using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Types;
using Assets.Crimson.Core.Components.Tags;
using Assets.Crimson.Core.Components.Tags.Weapons;
using Assets.Crimson.Core.Components.Weapons;
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
	public class WeaponSpawnBurst : TimerBaseBehaviour,
		IWeapon,
		IActorSpawnerAbility,
		IHasComponentName,
		ICooldownable,
		IBindable,
		IUseAimable,
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
		public string componentName = "";

		public WeaponType _weaponType;

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

		[Space, Header("Burst")]
		public bool UseBurst = false;

		[ShowIf(nameof(UseBurst)), MinMaxSlider(1, 30)] public Vector2Int _burstRange = new Vector2Int(1, 5);
		[ShowIf(nameof(UseBurst))] public float _burstDelay = .1f;

		[Space] public float projectileStartupDelay = 0f;

		[InfoBox("Time in seconds")]
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

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public MonoBehaviour[] PreShotAbilities;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public MonoBehaviour[] PostShotAbilities;

		[HideInInspector] public List<string> appliedPerksNames = new List<string>();

		[HideInInspector]
		public bool aimingByInput;

		public List<GameObject> SpawnedObjects { get; private set; }
		public List<Action<GameObject>> SpawnCallbacks { get; set; }
		public Action<GameObject> DisposableSpawnCallback { get; set; }

		public bool IsEnable
		{
			get => isEnable;
			set
			{
				isEnable = value;
				if (isEnable)
				{
					ActionsOnEnable.Execute();
				}
				else
				{
					ActionsOnDisable.Execute();
				}
			}
		}

		public float CooldownTime
		{
			get => cooldownTime;
			set => cooldownTime = value;
		}

		public int BindingIndex { get; set; } = -1;

		[ValidateInput(nameof(MustBeAimable), "Ability MonoBehaviours must derive from IAimable!")]
		public MonoBehaviour AimComponent;

		[Header("ActionsOnEnable")]
		public ActionsList ActionsOnEnable = new ActionsList();

		[Header("ActionsOnDisable")]
		public ActionsList ActionsOnDisable = new ActionsList();

		protected Entity _entity;
		private bool _actorToUi;
		private EntityManager _entityManager;

		private bool _isWaitingForShot;

		public event Action OnShot;

		private bool isEnable;
		private ActorAbilityList _preShotAbilities;
		private ActorAbilityList _postShotAbilities;

		public bool ActionExecutionAllowed { get; set; }
		public IAimable Aim => AimComponent as IAimable;

		public bool IsActivated { get; private set; }
		public bool OnHoldAttackActive { get; set; }

		public GameObject SpawnedAimingPrefab { get; set; }

		public Transform SpawnPointsRoot { get; private set; }
		protected EntityManager CurrentEntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

		public WeaponClip ClipData { get; private set; } = new WeaponClip();

		public WeaponType Type => _weaponType;

		private void Awake()
		{
			ActionsOnDisable.Init();
			ActionsOnEnable.Init();
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_preShotAbilities = new ActorAbilityList(PreShotAbilities);
			_postShotAbilities = new ActorAbilityList(PostShotAbilities);
			InitPool();

			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_entity = entity;
			ClipData.Setup(projectileClipCapacity, projectileClipCapacity);
			SpawnCallbacks = new List<Action<GameObject>>();

			_entityManager.AddComponent<TimerData>(entity);

			if (actorProjectileSpawnAnimProperties != null
				&& actorProjectileSpawnAnimProperties.HasActorProjectileAnimation)
			{
				_entityManager.AddComponentData(actor.Owner.ActorEntity, new ActorProjectileAnimData
				{
					AnimHash = Animator.StringToHash(actorProjectileSpawnAnimProperties.ActorProjectileAnimationName)
				});
			}

			appliedPerksNames = new List<string>();

			var playerActor = actor.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;
			_actorToUi = playerActor != null && playerActor.actorToUI;

			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}

			if (!_actorToUi)
			{
				return;
			}

			CreateSpawnPointsRoot();
			ResetSpawnPointRootRotation();

			if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnerPosition)
			{
				projectileSpawnData.SpawnPosition = SpawnPosition.UseSpawnPoints;
			}

			if (projectileSpawnData.SpawnPoints.Count > 0)
			{
				projectileSpawnData.SpawnPoints.Clear();
			}

			var baseSpawnPoint = new GameObject("Base Spawn Point");
			baseSpawnPoint.transform.SetParent(SpawnPointsRoot);

			baseSpawnPoint.transform.localPosition = Vector3.zero;
			baseSpawnPoint.transform.localRotation = Quaternion.identity;

			projectileSpawnData.SpawnPoints.Add(baseSpawnPoint);

			StartTimer();
		}

		public void Execute()
		{
			if (!IsEnable || !IsActivated || ClipData.IsEmpty || ClipData.Current == 0)
			{
				return;
			}

			if (projectileStartupDelay > 0)
			{
				if (_isWaitingForShot)
					return;
				_isWaitingForShot = true;
				Timer.TimedActions.AddAction(Shot, projectileStartupDelay);
			}
			else
			{
				Shot();
				Timer.TimedActions.AddAction(Execute, CooldownTime);
			}
		}

		public override void FinishTimer()
		{
			base.FinishTimer();
			RemoveSpawned();
		}

		public void InitPool()
		{
			projectileSpawnData.InitPool();
		}

		public void Reload()
		{
			CurrentEntityManager.AddComponentData(Actor.Owner.ActorEntity, new ReloadTag());
			Timer.TimedActions.AddAction(EndReload, clipReloadTime);
		}

		private void EndReload()
		{
			ClipData.Reload();
		}

		public void ResetSpawnPointRootRotation()
		{
			SpawnPointsRoot.localRotation = Quaternion.identity;
		}

		private void Shot()
		{
			_isWaitingForShot = false;
			_entityManager.AddComponentData(Actor.Owner.ActorEntity, new WeaponAttackTag());
			if (!UseBurst)
			{
				Spawn();
				return;
			}
			var shots = UnityEngine.Random.Range(_burstRange.x, _burstRange.y);
			for (var i = 0; i < shots; i++)
			{
				Timer.TimedActions.AddAction(Spawn, _burstDelay * i);
			}
			Timer.TimedActions.AddAction(FinishTimer, _burstDelay * shots);
		}

		public void Spawn()
		{
			CurrentEntityManager.AddComponentData(Actor.Owner.ActorEntity,
					new ActorProjectileThrowAnimTag());

			LookAtTargetIfAimExist();

			OnShot?.Invoke();
			ClipData.Decrease();
			_preShotAbilities.Execute();
			SpawnedObjects = ActorSpawn.Spawn(projectileSpawnData, Actor, Actor.Owner);
			_postShotAbilities.Execute();

			if (SpawnedObjects == null)
			{
				return;
			}

			InvokeSpawnCallbacks();

			SpawnedObjects.ForEach(go =>
			{
				DisposableSpawnCallback?.Invoke(go);
				DisposableSpawnCallback = null;
			});

			if (!_actorToUi)
			{
				return;
			}

			ResetSpawnPointRootRotation();
			OnHoldAttackActive = false;
		}

		public void StartFire()
		{
			IsActivated = true;
			Execute();
		}

		public void StopFire()
		{
			IsActivated = false;
			if (ClipData.Current == 0)
			{
				return;
			}
			if (projectileStartupDelay <= 0)
				ResetTimer();
		}

		private void CreateSpawnPointsRoot()
		{
			SpawnPointsRoot = new GameObject("spawn points root").transform;
			SpawnPointsRoot.SetParent(gameObject.transform);

			SpawnPointsRoot.localPosition = Vector3.zero;
		}

		private void InvokeSpawnCallbacks()
		{
			Action<GameObject> callback;
			for (var i = 0; i < SpawnCallbacks.Count; i++)
			{
				callback = SpawnCallbacks[i];
				SpawnedObjects.ForEach(go => callback.Invoke(go));
			}
		}

		private void LookAtTargetIfAimExist()
		{
			var aimTarget = Aim?.SpawnedAimingPrefab;
			if (aimTarget != null)
			{
				if (SpawnPointsRoot == null)
				{
					CreateSpawnPointsRoot();
				}
				SpawnPointsRoot.LookAt(aimTarget.transform);
				for (var i = 0; i < SpawnedObjects.Count; i++)
				{
					SpawnedObjects[i].transform.LookAt(aimTarget.transform);
				}
			}
		}

		private bool MustBeAimable(MonoBehaviour behaviour)
		{
			return behaviour is IActorAbility;
		}

		private void RemoveSpawned()
		{
			for (var i = 0; i < SpawnedObjects.Count; i++)
			{
				SpawnedObjects[i].Destroy();
			}
			SpawnedObjects.Clear();
		}

		private bool MustBeAbility(MonoBehaviour[] a)
		{
			return (a is null) || (a.All(s => s is IActorAbility));
		}
	}
}