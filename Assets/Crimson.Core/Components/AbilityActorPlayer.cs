using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Buffs;
using Assets.Crimson.Core.Common.ComponentDatas;
using Assets.Crimson.Core.Components.Tags;
using Assets.Crimson.Core.Components.Tags.Effects;
using Crimson.Core.Common;
using Crimson.Core.Components.Stats;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
	public struct ActorDeathAnimData : IComponentData
	{
		public int AnimHash;
	}

	public struct ActorForceAnimData : IComponentData
	{
		public int AnimHash;
	}

	public struct ActorStafeAnimData : IComponentData
	{
		public int AnimHash;
	}

	public struct ActorTakeDamageAnimData : IComponentData
	{
		public int AnimHash;
	}

	[HideMonoScript]
	[NetworkSimObject]
	public class AbilityActorPlayer : MonoBehaviour, IActorAbility, ITimer, ILevelable
	{
		[SerializeField] public PlayerStatsData _initStats;

		[TitleGroup("Player animation properties")]
		public ActorDeathAnimProperties actorDeathAnimProperties;

		[HideInInspector] public bool actorToUI;

		[Header("Additional Force Animation")]
		public ActorGeneralAnimProperties additionalForceAnim;

		[ShowIf(nameof(NeedCleanup))]
		public float corpseCleanupDelay;

		public DeadBehaviour deadActorBehaviour = new DeadBehaviour();

		[ReadOnly]
		public int deathCount;

		[TitleGroup("UI channel info")]
		[OnValueChanged(nameof(UpdateUIChannelInfo))]
		public bool ExplicitUIChannel;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public MonoBehaviour healAction;

		[HideInInspector] public bool isEnabled = true;

		[Space]
		[TitleGroup("Levelable properties")]
		[OnValueChanged(nameof(SetLevelableProperty))]
		public List<LevelableProperties> levelablePropertiesList = new List<LevelableProperties>();

		[TitleGroup("External behaviours")]
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public MonoBehaviour levelUpAction;

		[Header("Cleanup after death")]
		public bool NeedCleanup;

		[Header("Execute on death")]
		[ValidateInput(nameof(MustBeAbilities), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> DeathAbilities;

		[Header("Take Damage Animation")]
		public ActorGeneralAnimProperties TakeDamageAnimation;

		public string targetMarkActorComponentName;

		[ShowIf(nameof(ExplicitUIChannel))] public int UIChannelID = 0;

		[HideInInspector] public List<IActor> UIReceiverList = new List<IActor>();

		private EntityManager _entityManager;

		private Entity _entity;

		private List<FieldInfo> _levelablePropertiesInfoCached = new List<FieldInfo>();
		private IActorAbility _maxDistanceWeapon;
		private Dictionary<string, MemberInfo> _membersInfo = new Dictionary<string, MemberInfo>();
		private string _playerName;
		private PlayerStatsData _stats;
		private TimerComponent _timer;
		private List<IActorAbility> _deathAbilities;

		public IActor Actor { get; set; }

		[ShowInInspector] public int ActorId => Actor?.ActorId ?? 0;

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(CurrentEnergy))]
		public float CurrentEnergy => _stats.Energy.Current;

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(CurrentExperience))]
		public float CurrentExperience => _stats.CurrentExperience;

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(CurrentHealth))]
		[LevelableValue]
		public float CurrentHealth => _stats.Health.Current;

		public bool IsAlive => !_entityManager.HasComponent<DeadActorTag>(_entity);

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(Level))]
		public int Level
		{
			get => _stats.Level;
			set
			{
				_stats.Level = value;
				if (_entity != Entity.Null)
				{
					_entityManager.SetComponentData(_entity, _stats);
				}
			}
		}

		public List<FieldInfo> LevelablePropertiesInfoCached
		{
			get
			{
				if (_levelablePropertiesInfoCached.Count == 0)
				{
					_levelablePropertiesInfoCached = this.GetFieldsWithAttributeInfo<LevelableValue>();
				}
				return _levelablePropertiesInfoCached;
			}
		}

		public List<LevelableProperties> LevelablePropertiesList
		{
			get => levelablePropertiesList;
			set => levelablePropertiesList = value;
		}

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(LevelUpRequiredExperience))]
		[LevelableValue]
		public int LevelUpRequiredExperience => _stats.LevelUpRequiredExperience;

		public IActorAbility MaxDistanceWeapon
		{
			get
			{
				return !ReferenceEquals(_maxDistanceWeapon, null)
					? _maxDistanceWeapon
					: Actor.Abilities.Where(a => a is AbilityWeapon)
					.OrderByDescending(w => ((AbilityWeapon)w).findTargetProperties.maxDistanceThreshold)
					.FirstOrDefault();
			}
		}

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(MaxEnergy))]
		[LevelableValue]
		public float MaxEnergy => _stats.Energy.MaxLimit;

		[NetworkSimData]
		[CastToUI(nameof(MaxHealth))]
		[LevelableValue]
		public float MaxHealth => _stats.Health.MaxLimit;

		[TitleGroup("Player data")]
		[NetworkSimData]
		[CastToUI("Name")]
		[InfoBox("32 symbols max")]
		public string PlayerName
		{
			get => _playerName;
			set
			{
				_playerName = value;
				UpdateUIData("Name");
			}
		}

		public PlayerStatsData Stats
		{
			get => _stats;
			set
			{
				_stats = value;
				if (_entity != Entity.Null)
				{
					if (_entityManager.HasComponent<PlayerStatsData>(_entity))
					{
						_entityManager.SetComponentData(_entity, value);
					}
					else
					{
						_entityManager.AddComponentData(_entity, value);
					}
				}
			}
		}

		public TimerComponent Timer => _timer = this.gameObject.GetOrCreateTimer(_timer);

		public bool TimerActive
		{
			get => isEnabled;
			set => isEnabled = value;
		}

		[NetworkSimData]
		[ReadOnly]
		[CastToUI(nameof(TotalDamageApplied))]
		public int TotalDamageApplied => _stats.TotalDamageApplied;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			Actor.Owner = Actor;
			_deathAbilities = DeathAbilities.Where(s => s is IActorAbility).Cast<IActorAbility>().ToList();

			_membersInfo = new Dictionary<string, MemberInfo>();

			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_entityManager.AddBuffer<HealthPercentPerTimeBuffData>(_entity);
			_entityManager.AddBuffer<EnergyPercentPerTimeBuffData>(_entity);

			var stats = _initStats;
			stats.LevelUpRequiredExperience = GameMeta.PointsToLevelUp;
			Stats = stats;

			UIReceiverList = new List<IActor>();

			_timer = gameObject.GetOrCreateTimer(_timer);

			_entityManager.AddComponent<TimerData>(entity);

			if (TakeDamageAnimation.HasAnimation)
			{
				_entityManager.AddComponentData(entity, new ActorTakeDamageAnimData
				{
					AnimHash = TakeDamageAnimation.AnimationHash
				});
			}

			if (additionalForceAnim.HasAnimation)
			{
				_entityManager.AddComponentData(entity, new ActorForceAnimData
				{
					AnimHash = additionalForceAnim.AnimationHash
				});
			}

			if (actorDeathAnimProperties.HasActorDeathAnimation)
			{
				_entityManager.AddComponentData(entity, new ActorDeathAnimData
				{
					AnimHash = Animator.StringToHash(actorDeathAnimProperties.ActorDeathAnimationName)
				});
			}

			foreach (var info in typeof(AbilityActorPlayer).GetMembers()
				.Where(field => field.GetCustomAttribute<CastToUI>(false) != null))
			{
				_membersInfo.Add(info.Name, info);
			}

			var playerInput = GetComponent<AbilityPlayerInput>();

			actorToUI = playerInput != null && playerInput.inputSource == InputSource.UserInput;

			if (!actorToUI)
			{
				return;
			}

			_entityManager.AddComponent<ApplyPresetPerksData>(Actor.ActorEntity);
		}

		public void Death()
		{
			if (_entityManager.Exists(_entity))
			{
				_entityManager.AddComponent<ImmediateDestructionActorTag>(_entity);
			}
		}

		public void Execute()
		{
		}

		public void FinishTimer()
		{
			Death();
		}

		public void ForceUpdatePlayerUIData()
		{
			foreach (var memberInfo in _membersInfo)
			{
				UpdateUIData(memberInfo.Key);
			}
		}

		public void LevelUp()
		{
			_stats.CurrentExperience -= LevelUpRequiredExperience;

			if (levelUpAction != null)
			{
				((IActorAbility)levelUpAction).Execute();
			}

			if (actorToUI)
			{
				SetLevel(Level + 1);
				_entityManager.AddComponent<PerksSelectionAvailableTag>(_entity);
			}
			else
			{
				Level++;
			}

			UpdateUIData(nameof(Level));
			UpdateUIData(nameof(CurrentExperience));
		}

		public void RemoveUIElements()
		{
			for (var i = 0; i < UIReceiverList.Count; i++)
			{
				var element = UIReceiverList[i];
				if (_entityManager.Exists(element.ActorEntity))
				{
					_entityManager.AddComponent<ImmediateDestructionActorTag>(element.ActorEntity);
				}
			}
		}

		public void SetLevel(int level)
		{
			this.SetAbilityLevel(level, LevelablePropertiesInfoCached, Actor);

			foreach (var ability in Actor.Abilities.Where(a => a is ILevelable && !ReferenceEquals(a, this)))
			{
				((ILevelable)ability).SetLevel(Level);
			}
		}

		public void SetLevelableProperty()
		{
			this.SetLevelableProperty(LevelablePropertiesInfoCached);
		}

		public void StartDeathTimer()
		{
			RemoveUIElements();

			StartTimer();
		}

		public void StartTimer()
		{
			Timer.TimedActions.AddAction(FinishTimer, corpseCleanupDelay);
		}

		public void UpdateEnergy(float delta)
		{
			if (!IsAlive)
			{
				return;
			}

			var playerStats = _entityManager.GetComponentData<PlayerStatsData>(_entity);

			playerStats.Energy.Current += delta;

			Stats = playerStats;
			UpdateUIData(nameof(CurrentEnergy));
		}

		public void UpdateExperience(int delta)
		{
			if (delta == 0)
			{
				return;
			}

			var playerStats = _entityManager.GetComponentData<PlayerStatsData>(_entity);

			_stats.CurrentExperience += delta;
			while (CurrentExperience >= LevelUpRequiredExperience)
			{
				LevelUp();
			}

			playerStats.CurrentExperience = CurrentExperience;
			Stats = playerStats;

			UpdateUIData(nameof(LevelUpRequiredExperience));
			UpdateUIData(nameof(CurrentExperience));
		}

		public void UpdateHealth(float delta)
		{
			if (!IsAlive)
			{
				SetOverdamage(math.abs(delta));
				return;
			}

			var playerStats = _entityManager.GetComponentData<PlayerStatsData>(_entity);
			playerStats.Health.Current += delta;

			Stats = playerStats;

			if (delta > 0 && healAction != null)
			{
				((IActorAbility)healAction).Execute();
				_entityManager.AddComponentData(_entity, new HealedActorTag());
			}

			if (delta < 0)
			{
				_entityManager.AddComponentData(_entity, new DamageFXTag());
				_entityManager.AddComponentData(_entity, new ShakeFXTag());
				_entityManager.AddComponentData(_entity, new DamagedActorTag());
			}

			UpdateUIData(nameof(CurrentHealth));

			if (Stats.Health.MinLimit == Stats.Health.Current)
			{
				deathCount++;
				_entityManager.RemoveComponent<OverdamageData>(Actor.ActorEntity);
				if (_deathAbilities != null)
				{
					for (var i = 0; i < _deathAbilities.Count; i++)
					{
						_deathAbilities[i].Execute();
					}
				}
				_entityManager.AddComponent<DeathAnimationTag>(Actor.ActorEntity);
				_entityManager.AddComponent<DeadActorTag>(Actor.ActorEntity);
			}
		}

		public void UpdateMaxEnergy(int delta)
		{
			if (!IsAlive)
			{
				return;
			}

			var playerStats = _entityManager.GetComponentData<PlayerStatsData>(_entity);

			playerStats.Energy.MaxLimit += delta;
			playerStats.Energy.Current += delta;

			_entityManager.SetComponentData(_entity, playerStats);

			UpdateUIData(nameof(CurrentEnergy));
			UpdateUIData(nameof(MaxEnergy));
		}

		public void UpdateMaxHealthData(int delta)
		{
			if (delta == 0)
			{
				return;
			}

			var playerStats = _entityManager.GetComponentData<PlayerStatsData>(_entity);

			playerStats.Health.MaxLimit += delta;

			Stats = playerStats;

			UpdateUIData(nameof(MaxHealth));
		}

		public void UpdateTotalDamageData(int delta)
		{
			if (delta == 0)
			{
				return;
			}

			var playerStats = _entityManager.GetComponentData<PlayerStatsData>(_entity);

			playerStats.TotalDamageApplied += delta;

			Stats = playerStats;
			UpdateUIData(nameof(TotalDamageApplied));
		}

		internal void UpdateMaxEnergy(float value)
		{
			throw new System.NotImplementedException();
		}

		private bool MustBeAbility(MonoBehaviour a)
		{
			return (a is IActorAbility) || (a is null);
		}

		private bool MustBeAbilities(List<MonoBehaviour> items)
		{
			return items.All(s => s is IActorAbility) || items.Count == 0;
		}

		private void SetOverdamage(float value)
		{
			if (Actor == null)
			{
				return;
			}
			var absValue = math.abs(value);
			if (_entityManager.HasComponent<OverdamageData>(Actor.ActorEntity))
			{
				var data = _entityManager.GetComponentData<OverdamageData>(Actor.ActorEntity);
				data.Damage += absValue;
				_entityManager.SetComponentData(_entity, data);
			}
			else if (Actor.ActorEntity != Entity.Null)
			{
				_entityManager.AddComponentData(Actor.ActorEntity, new OverdamageData(absValue));
			}

			_entityManager.AddComponentData(_entity, new OverdamageFXTag());
		}

		private void UpdateUIChannelInfo()
		{
			if (!ExplicitUIChannel)
			{
				UIChannelID = 0;
			}
		}

		private void UpdateUIData(string memberName)
		{
			foreach (UIReceiver receiver in UIReceiverList.Where(receiver => _membersInfo.ContainsKey(memberName) && receiver is UIReceiver))
			{
				var info = _membersInfo[memberName];
				object value;
				switch (info.MemberType)
				{
					case MemberTypes.Field:
						value = ((FieldInfo)info).GetValue(this);
						break;

					case MemberTypes.Property:
						value = ((PropertyInfo)info).GetValue(this);
						break;

					default:
						continue;
				}
				var id = info.GetCustomAttribute<CastToUI>(false).FieldId;
				receiver.UpdateUIElementsData(id, value);
			}
		}
	}
}