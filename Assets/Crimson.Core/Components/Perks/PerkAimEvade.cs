using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Loading;
using Crimson.Core.Utils;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Perks
{
	public class PerkAimEvade : CooldownBehaviour,
		IActorAbility,
		IPerkAbility,
		ICooldownable
	{
		public Entity _entity;
		public EntityManager _entityManager;
		public List<GameObject> _perkEffects = new List<GameObject>();
		public List<GameObject> _postEffects = new List<GameObject>();
		public ActorGeneralAnimProperties AnimProperties;
		public float cooldownTime;
		public float Distance;
		public float Duration = 1;
		public List<MonoBehaviour> perkRelatedComponents = new List<MonoBehaviour>();
		public float PositionThreshold;
		private IActor _target;
		public IActor Actor { get; set; }

		public float CooldownTime
		{
			get => cooldownTime;
			set => cooldownTime = value;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (AnimProperties.HasAnimation)
			{
				_entityManager.AddComponentData(entity, new ActorStafeAnimData
				{
					AnimHash = AnimProperties.AnimationHash
				});
			}

			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Apply(IActor target)
		{
			if (!IsEnable)
			{
				return;
			}
			_target = target;

			ApplyActionWithCooldown(cooldownTime, Strafe);
		}

		public void Execute()
		{
			Apply(Actor);
		}

		public override void FinishTimer()
		{
			base.FinishTimer();
			this.FinishAbilityCooldownTimer(Actor);
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

		public override void StartTimer()
		{
			base.StartTimer();
			this.StartAbilityCooldownTimer(Actor);
		}

		public void Strafe()
		{
			var direction = transform.right;
			if (Random.Range(0, 2) > 0)
			{
				direction *= -1;
			}
			direction = direction.normalized * Distance;
			var moveData = new MoveData
			{
				EndPosition = transform.position + direction,
				PositionThreshold = PositionThreshold,
				Velocity = direction.magnitude / Duration
			};
			_entityManager.AddComponentData(_entity, moveData);
			_entityManager.AddComponent<StrafeActorTag>(Actor.ActorEntity);

			if (_perkEffects != null && _perkEffects.Count > 0)
			{
				var spawnData = new ActorSpawnerSettings
				{
					objectsToSpawn = _perkEffects,
					SpawnPosition = SpawnPosition.UseSpawnerPosition,
					parentOfSpawns = TargetType.None,
					runSpawnActionsOnObjects = true,
					destroyAbilityAfterSpawn = true
				};

				ActorSpawn.Spawn(spawnData, Actor, null);
			}

			Timer.TimedActions.AddAction(() =>
			{
				if (_postEffects != null && _postEffects.Count > 0)
				{
					var spawnData = new ActorSpawnerSettings
					{
						objectsToSpawn = _postEffects,
						SpawnPosition = SpawnPosition.UseSpawnerPosition,
						parentOfSpawns = TargetType.None,
						runSpawnActionsOnObjects = true,
						destroyAbilityAfterSpawn = true
					};

					ActorSpawn.Spawn(spawnData, Actor, null);
				}
			}, Duration);
		}
	}
}