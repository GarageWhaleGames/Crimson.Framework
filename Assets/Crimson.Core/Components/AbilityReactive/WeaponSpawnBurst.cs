﻿using Assets.Crimson.Core.Components.Weapons;
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
		IComponentName,
		ICooldownable,
		IBindable,
		IUseAimable
	{
		public float _cooldownTime = 0.3f;

		[InfoBox("Time in seconds")]
		public float _weaponFinishTime = 0.150f;

		public ActorProjectileSpawnAnimProperties actorProjectileSpawnAnimProperties;

		[ValidateInput(nameof(MustBeAimable), "Ability MonoBehaviours must derive from IAimable!")]
		public MonoBehaviour AimComponent;

		[HideInInspector] public List<string> appliedPerksNames = new List<string>();

		[HideIf(nameof(projectileClipCapacity), 0f)]
		[Space]
		public List<MonoBehaviour> clipReloadDisplayToggle = new List<MonoBehaviour>();

		[HideIf(nameof(projectileClipCapacity), 0f)] public float clipReloadTime = 1f;
		public string componentName = "";

		[InfoBox("Clip Capacity of 0 stands for unlimited clip")]
		public int projectileClipCapacity = 0;

		public ActorSpawnerSettings projectileSpawnData;

		[InfoBox("Put here IEnable implementation to display reload")]
		[Space]
		public List<MonoBehaviour> reloadDisplayToggle = new List<MonoBehaviour>();

		public bool suppressWeaponSpawn = false;

		protected Entity _entity;
		private bool _actorToUi;
		private EntityManager _dstManager;
		private int _projectileClip;

		public bool ActionExecutionAllowed { get; set; }
		public IActor Actor { get; set; }
		public IAimable Aim => AimComponent as IAimable;
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

		public bool IsActivated { get; private set; }
		public bool IsEnable { get; set; }
		public bool OnHoldAttackActive { get; set; }

		public List<Action<GameObject>> SpawnCallbacks { get; set; }

		public GameObject SpawnedAimingPrefab { get; set; }

		public List<GameObject> SpawnedObjects { get; private set; } = new List<GameObject>();

		public Transform SpawnPointsRoot { get; private set; }
		protected EntityManager CurrentEntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			InitPool();

			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_entity = entity;
			_projectileClip = projectileClipCapacity;
			SpawnCallbacks = new List<Action<GameObject>>();

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

			CreateSpawnPointsRoot();
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
			if (!IsEnable)
			{
				return;
			}

			if (IsActivated && _projectileClip > 0 && CurrentEntityManager.Exists(_entity))
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
			_projectileClip = projectileClipCapacity;
		}

		public void ResetSpawnPointRootRotation()
		{
			SpawnPointsRoot.localRotation = Quaternion.identity;
		}

		public void Spawn()
		{
			CurrentEntityManager.AddComponentData(_entity,
					new ActorProjectileThrowAnimTag());

			LookAtTargetIfAimExist();

			_projectileClip--;

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