using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Perks
{
	public class PerkZigZagDash : CooldownBehaviour, IActorAbility, IPerkAbility, ILevelable,
		ICooldownable
	{
		[ReadOnly] public int perkLevel = 1;
		public float DashDelay;
		public int DashCount = 4;
		[LevelableValue] public float force = 25;

		public float cooldownTime;

		public List<GameObject> dashFX = new List<GameObject>();

		public List<MonoBehaviour> perkRelatedComponents = new List<MonoBehaviour>();

		[Space]
		[TitleGroup("Levelable properties")]
		[OnValueChanged("SetLevelableProperty")]
		public List<LevelableProperties> levelablePropertiesList = new List<LevelableProperties>();

		public IActor Actor { get; set; }
		public bool ActionExecutionAllowed { get; set; }

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
				return _levelablePropertiesInfoCached.Count != 0
					? _levelablePropertiesInfoCached
					: (_levelablePropertiesInfoCached = this.GetFieldsWithAttributeInfo<LevelableValue>());
			}
		}

		public float CooldownTime
		{
			get => cooldownTime;
			set => cooldownTime = value;
		}

		private List<FieldInfo> _levelablePropertiesInfoCached = new List<FieldInfo>();
		private Vector3 _previousVelocity;
		private IActor _target;
		private Vector3[] _dashVectors;
		private int _currentDash;
		private EntityManager _entityManager;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;

			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Execute()
		{
			Apply(Actor);
		}

		public void Apply(IActor target)
		{
			_target = target;
			ApplyActionWithCooldown(CooldownTime, StartDash);
		}

		private void StartDash()
		{
			_dashVectors = GetDashVectors();
			_currentDash = 0;

			for (var i = 0; i < _dashVectors.Length; i++)
			{
				this.AddAction(ApplyDash, DashDelay * i);
			}
			Timer.TimedActions.AddAction(() =>
			{
				_dashVector = Vector3.zero;
			}, DashDelay * _dashVectors.Length);
		}

		public void ApplyDash()
		{
			var rotation = transform.rotation.eulerAngles;
			var yRotation = Quaternion.AngleAxis(rotation.y, transform.up);
			var dashVector = yRotation * _dashVectors[_currentDash];
			_currentDash++;
			if (_target != Actor.Owner)
			{
				var ownerActorPlayer =
					Actor.Owner.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;

				if (ownerActorPlayer == null)
				{
					return;
				}

				this.SetAbilityLevel(ownerActorPlayer.Level, LevelablePropertiesInfoCached, Actor, _target);
			}

			if (!_target.AppliedPerks.Contains(this))
			{
				_target.AppliedPerks.Add(this);
			}

			var movement = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<ActorMovementData>(_target.ActorEntity);

			movement.Input = dashVector;
			World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(_target.ActorEntity, movement);

			transform.position += dashVector;

			if (dashFX != null && dashFX.Count > 0)
			{
				var spawnData = new ActorSpawnerSettings
				{
					objectsToSpawn = dashFX,
					SpawnPosition = SpawnPosition.UseSpawnerPosition,
					RotationOfSpawns = RotationOfSpawns.UseSpawnPointRotation,
					parentOfSpawns = TargetType.None,
					runSpawnActionsOnObjects = true,
					destroyAbilityAfterSpawn = true
				};

				var fx = ActorSpawn.Spawn(spawnData, Actor, null)?.First();
			}
		}

		public void SetLevel(int level)
		{
			this.SetAbilityLevel(level, LevelablePropertiesInfoCached, Actor);
		}

		public void Remove()
		{
			if (_target != null && _target.AppliedPerks.Contains(this))
			{
				_target.AppliedPerks.Remove(this);
			}

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

		public void SetLevelableProperty()
		{
			this.SetLevelableProperty(LevelablePropertiesInfoCached);
		}

		private void OnDrawGizmosSelected()
		{
			if (DashCount == 0)
			{
				return;
			}
			var vectors = GetDashVectors();

			var position = Vector3.zero;
			var axis = Vector3.forward;
			Gizmos.matrix = transform.localToWorldMatrix;
			for (var i = 0; i < vectors.Length; i++)
			{
				Gizmos.DrawLine(position, vectors[i] + position);
				position += Vector3.Project(vectors[i], axis);
			}
		}

		private Vector3[] GetDashVectors()
		{
			var positions = new Vector3[DashCount];
			var additionalVector = Vector3.right;
			for (var i = 0; i < positions.Length; i++)
			{
				var direction = force * Vector3.forward;
				direction += additionalVector;
				positions[i] = direction;
				additionalVector *= -1;
			}

			return positions;
		}
	}
}