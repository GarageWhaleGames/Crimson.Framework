using Assets.Crimson.Core.Common;
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

		public float _cooldownTime = 0.3f;

		[InfoBox("Time in seconds")]
		public float _weaponFinishTime = 0.150f;

		[ValidateInput(nameof(MustBeAimable), "Ability MonoBehaviours must derive from IAimable!")]
		public MonoBehaviour AimComponent;

		public ActionsList ActionsOnEnable = new ActionsList();

		public ActionsList ActionsOnDisable = new ActionsList();

		protected Entity _entity;
		private bool _actorToUi;
		private EntityManager _dstManager;

		public event Action OnShot;

		private bool isEnable;

		public bool ActionExecutionAllowed { get; set; }
		public IAimable Aim => AimComponent as IAimable;

		public bool IsActivated { get; private set; }
		public bool OnHoldAttackActive { get; set; }

		public GameObject SpawnedAimingPrefab { get; set; }

		public Transform SpawnPointsRoot { get; private set; }
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
			InitPool();

			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_entity = entity;
			ClipData.Setup(projectileClipCapacity, projectileClipCapacity);
			SpawnCallbacks = new List<Action<GameObject>>();

			_dstManager.AddComponent<TimerData>(entity);

			if (actorProjectileSpawnAnimProperties != null
				&& actorProjectileSpawnAnimProperties.HasActorProjectileAnimation)
			{
				_dstManager.AddComponentData(actor.Owner.ActorEntity, new ActorProjectileAnimData
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
		}

		public void Execute()
		{
			if (!IsEnable || !IsActivated || ClipData.IsEmpty)
			{
				return;
			}

			if (projectileStartupDelay > 0)
			{
				Timer.TimedActions.AddAction(Spawn, projectileStartupDelay);
			}
			else
			{
				Spawn();

				StartTimer();
				this.RestartAction(Execute, _weaponFinishTime);
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
			ClipData.Reload();
		}

		public void ResetSpawnPointRootRotation()
		{
			SpawnPointsRoot.localRotation = Quaternion.identity;
		}

		public void Spawn()
		{
			CurrentEntityManager.AddComponentData(Actor.Owner.ActorEntity,
					new ActorProjectileThrowAnimTag());

			LookAtTargetIfAimExist();

			ClipData.Decrease();
			OnShot?.Invoke();
			SpawnedObjects = ActorSpawn.Spawn(projectileSpawnData, Actor, Actor.Owner);

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
	}
}