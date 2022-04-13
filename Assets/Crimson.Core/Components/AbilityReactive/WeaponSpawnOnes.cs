using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components.Tags;
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
using UnityEngine.InputSystem;

namespace Crimson.Core.Components.AbilityReactive
{
	[HideMonoScript]
	public class WeaponSpawnOnes : TimerBaseBehaviour,
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

		public Transform SpawnPointsRoot { get; private set; }

		[ValidateInput(nameof(MustBeAimable), "Ability MonoBehaviours must derive from IAimable!")]
		public MonoBehaviour AimComponent;

		public ActionsList ActionsOnEnable = new ActionsList();

		public ActionsList ActionsOnDisable = new ActionsList();

		public ActorGeneralAnimProperties reloadAnimProps;

		protected Entity _entity;
		private bool _actorToUi;
		private EntityManager _dstManager;
		private bool isEnable;

		public event Action OnShot;

		public bool ActionExecutionAllowed { get; set; }
		public IAimable Aim => AimComponent as IAimable;
		public bool OnHoldAttackActive { get; set; }
		public GameObject SpawnedAimingPrefab { get; set; }

		protected EntityManager CurrentEntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

		public WeaponClip ClipData { get; private set; } = new WeaponClip();

		private void Awake()
		{
			ActionsOnDisable.Init();
			ActionsOnEnable.Init();
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			SpawnCallbacks = new List<Action<GameObject>>();
			ClipData.Setup(projectileClipCapacity, projectileClipCapacity);
			_dstManager.AddComponent<TimerData>(entity);
			IsEnable = true;

			if (actorProjectileSpawnAnimProperties != null
				&& actorProjectileSpawnAnimProperties.HasActorProjectileAnimation)
			{
				_dstManager.AddComponentData(Actor.Owner.ActorEntity, new ActorProjectileAnimData
				{
					AnimHash = Animator.StringToHash(actorProjectileSpawnAnimProperties.ActorProjectileAnimationName)
				});
			}

			if (reloadAnimProps.HasAnimation)
			{
				_dstManager.AddComponentData(Actor.Owner.ActorEntity, new ReloadAnimationData
				{
					AnimHash = reloadAnimProps.AnimationHash
				});
			}

			appliedPerksNames = new List<string>();

			SpawnPointsRoot = new GameObject("spawn points root").transform;
			SpawnPointsRoot.SetParent(gameObject.transform);

			SpawnPointsRoot.localPosition = Vector3.zero;
			ResetSpawnPointRootRotation();

			if (projectileSpawnData.SpawnPosition == SpawnPosition.UseSpawnerPosition)
			{
				projectileSpawnData.SpawnPosition = SpawnPosition.UseSpawnPoints;
			}

			if (projectileSpawnData.SpawnPoints.Any())
			{
				projectileSpawnData.SpawnPoints.Clear();
			}

			var baseSpawnPoint = new GameObject("Base Spawn Point");
			baseSpawnPoint.transform.SetParent(SpawnPointsRoot);

			baseSpawnPoint.transform.localPosition = Vector3.zero;
			baseSpawnPoint.transform.localRotation = Quaternion.identity;

			projectileSpawnData.SpawnPoints.Add(baseSpawnPoint);

			InitPool();

			var playerActor = actor.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;
			_actorToUi = playerActor != null && playerActor.actorToUI;

			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Execute()
		{
			if (!IsEnable || ClipData.IsEmpty)
			{
				return;
			}

			if (projectileStartupDelay > 0)
			{
				Timer.TimedActions.AddAction(Shot, projectileStartupDelay);
			}
			else
			{
				Shot();
			}
		}

		private void Shot()
		{
			Spawn();

			CurrentEntityManager.AddComponentData(Actor.Owner.ActorEntity,
				new ActorProjectileThrowAnimTag());

			ClipData.Decrease();
			OnShot?.Invoke();
			if (CooldownTime.Equals(0))
			{
				return;
			}

			StartTimer();
			Timer.TimedActions.AddAction(FinishTimer, CooldownTime);
		}

		public override void FinishTimer()
		{
			IsEnable = true;
			base.FinishTimer();
			this.FinishAbilityCooldownTimer(Actor);
		}

		public void InitPool()
		{
			projectileSpawnData.InitPool();
		}

		public void Reload()
		{
			CurrentEntityManager.AddComponentData(_entity, new ReloadTag());
			ClipData.Reload();
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

			if (!_actorToUi)
			{
				return;
			}

			ResetSpawnPointRootRotation();
			OnHoldAttackActive = false;
		}

		public void StartFire()
		{
			Execute();
		}

		public override void StartTimer()
		{
			IsEnable = false;

			base.StartTimer();
			this.StartAbilityCooldownTimer(Actor);
		}

		public void StopFire()
		{
		}

		private bool MustBeAimable(MonoBehaviour behaviour)
		{
			return behaviour is IActorAbility;
		}
	}
}