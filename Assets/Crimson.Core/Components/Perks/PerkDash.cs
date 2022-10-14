using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Crimson.Core.Components.Perks
{
	[HideMonoScript]
	public class PerkDash : CooldownBehaviour, IActorAbility, IPerkAbility, IPerkAbilityBindable, ILevelable,
		ICooldownable, IAimable
	{
		[Sirenix.OdinInspector.ReadOnly] public int perkLevel = 1;
		[LevelableValue] public float force = 25;
		public float timer = 0.1f;

		public float cooldownTime;

		public bool aimingAvailable;
		[HideIf("aimingAvailable")] public bool useMovementVector = true;
		public bool deactivateAimingOnCooldown;

		public AimingProperties aimingProperties;
		public AimingAnimationProperties aimingAnimProperties;

		public List<GameObject> dashFX = new List<GameObject>();

		public List<MonoBehaviour> perkRelatedComponents = new List<MonoBehaviour>();

		[Space]
		[TitleGroup("Levelable properties")]
		[OnValueChanged("SetLevelableProperty")]
		public List<LevelableProperties> levelablePropertiesList = new List<LevelableProperties>();

		public IActor Actor { get; set; }
		public bool ActionExecutionAllowed { get; set; }
		public GameObject SpawnedAimingPrefab { get; set; }

		private Vector3 _dashVector = new Vector3();

		public List<MonoBehaviour> PerkRelatedComponents
		{
			get
			{
				perkRelatedComponents.RemoveAll(c => ReferenceEquals(c, null));
				return perkRelatedComponents;
			}
			set => perkRelatedComponents = value;
		}

		public int Level
		{
			get => perkLevel;
			set => perkLevel = value;
		}

		public List<LevelableProperties> LevelablePropertiesList
		{
			get => levelablePropertiesList;
			set => levelablePropertiesList = value;
		}

		public List<FieldInfo> LevelablePropertiesInfoCached
		{
			get
			{
				if (_levelablePropertiesInfoCached.Any()) return _levelablePropertiesInfoCached;
				return _levelablePropertiesInfoCached = this.GetFieldsWithAttributeInfo<LevelableValue>();
			}
		}

		public float CooldownTime
		{
			get => cooldownTime;
			set => cooldownTime = value;
		}

		public int BindingIndex { get; set; } = -1;

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

		public bool OnHoldAttackActive { get; set; }

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

		private List<FieldInfo> _levelablePropertiesInfoCached = new List<FieldInfo>();
		private Vector3 _previousVelocity;
		private IActor _target;

		private bool _aimingActive;

		private EntityManager _entityManager;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;

			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (AimingAnimProperties.HasActorAimingAnimation)
			{
				_entityManager.AddComponentData(entity, new AimingAnimProperties
				{
					AnimHash = Animator.StringToHash(AimingAnimProperties.ActorAimingAnimationName)
				});
			}

			if (!Actor.Abilities.Contains(this)) Actor.Abilities.Add(this);
		}

		public void Execute()
		{
			Apply(Actor);
		}

		public void Apply(IActor target)
		{
			_target = target;

			ApplyActionWithCooldown(cooldownTime, ApplyDash);
		}

		public void ApplyDash()
		{
			if (_target != Actor.Owner)
			{
				var ownerActorPlayer =
					Actor.Owner.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;

				if (ownerActorPlayer == null) return;

				this.SetAbilityLevel(ownerActorPlayer.Level, LevelablePropertiesInfoCached, Actor, _target);
			}

			if (!_target.AppliedPerks.Contains(this)) _target.AppliedPerks.Add(this);

			var navMeshAgent = _target.GameObject.GetComponent<NavMeshAgent>();
			var hasAgent = navMeshAgent != null;
			var targetRigidbody = _target.GameObject.GetComponent<Rigidbody>();

			if (targetRigidbody == null && !hasAgent) return;

			_previousVelocity = hasAgent ? navMeshAgent.velocity : targetRigidbody.velocity;

			var dashVector = new Vector3();

			var movement = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<ActorMovementData>(_target.ActorEntity);

			if (_aimingActive)
			{
				switch (AimingProperties.aimingType)
				{
					case AimingType.AimingArea:
						dashVector = _dashVector;
						break;

					case AimingType.SightControl:
						transform.LookAt(SpawnedAimingPrefab.transform);
						dashVector = _target.GameObject.transform.forward;
						break;
				}
			}
			else
			{
				dashVector = useMovementVector ? (Vector3)movement.MovementCache : _target.GameObject.transform.forward;
			}

			movement.Input = dashVector * force;
			World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(_target.ActorEntity, movement);

			if (hasAgent)
			{
				navMeshAgent.velocity = dashVector * force;
			}
			else
			{
				targetRigidbody.AddForce(dashVector * force, ForceMode.VelocityChange);
			}

			if (dashFX != null && dashFX.Count > 0)
			{
				var spawnData = new ActorSpawnerSettings
				{
					objectsToSpawn = dashFX,
					SpawnPosition = SpawnPosition.UseSpawnerPosition,
					RotationOfSpawns = useMovementVector ? RotationOfSpawns.SpawnerMovement : RotationOfSpawns.UseSpawnPointRotation,
					parentOfSpawns = TargetType.None,
					runSpawnActionsOnObjects = true,
					destroyAbilityAfterSpawn = true
				};

				var fx = ActorSpawn.Spawn(spawnData, Actor, null)?.First();
			}

			Timer.TimedActions.AddAction(() =>
			{
				if (hasAgent)
				{
					navMeshAgent.velocity = _previousVelocity;
				}
				else if (targetRigidbody == null)
				{
					targetRigidbody.velocity = _previousVelocity;
				}
				_dashVector = Vector3.zero;
			}, timer);

			_aimingActive = false;
		}

		public void SetLevel(int level)
		{
			this.SetAbilityLevel(level, LevelablePropertiesInfoCached, Actor);
		}

		public void Remove()
		{
			if (_target != null && _target.AppliedPerks.Contains(this)) _target.AppliedPerks.Remove(this);

			foreach (var component in perkRelatedComponents)
			{
				Destroy(component);
			}

			Destroy(this);
		}

		public override void FinishTimer()
		{
			base.FinishTimer();
			this.FinishAbilityCooldownTimer(Actor);
		}

		public override void StartTimer()
		{
			base.StartTimer();
			this.StartAbilityCooldownTimer(Actor);
		}

		public void EvaluateAim(Vector2 pos)
		{
			_aimingActive = true;
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
			}
		}

		public void EvaluateAimByArea(Vector2 pos)
		{
			_dashVector = Quaternion.Euler(0, -180, 0) * AbilityUtils.EvaluateAimByArea(this, pos);
		}

		public void EvaluateAimBySight(Vector2 pos)
		{
			this.EvaluateAimBySight(Actor, pos);
		}

		public void ResetAiming()
		{
			this.ResetAiming(Actor);
		}

		public void SetLevelableProperty()
		{
			this.SetLevelableProperty(LevelablePropertiesInfoCached);
		}
	}
}