using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityActorTagChecker : TimerBaseBehaviour, IActorAbility
	{
		[Header("Execute On Start")]
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		[PropertyOrder(5)]
		public List<MonoBehaviour> ActionsOnStart = new List<MonoBehaviour>();

		[Header("Execute On Success")]
		[PropertyOrder(6)]
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> ActionsOnSuccess = new List<MonoBehaviour>();

		[PropertyOrder(2)] public float CheckupDelay;
		[PropertyOrder(0)] public bool ExecuteOnAwake;

		[ValueDropdown(nameof(GetComponentTypes))]
		[PropertyOrder(3)] public string TagName;

		[PropertyOrder(1)] public float UpdateTime;
		private IEnumerable<IActorAbility> _abilitiesOnStart;
		private IEnumerable<IActorAbility> _abilitiesOnSuccess;
		private Type _checkType;
		private Entity _entity;
		private EntityManager _entityManager;
		public IActor Actor { get; set; }
		public bool IsEnable { get; set; } = false;

		private Type[] Types => Assembly.GetExecutingAssembly().GetTypes();

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			if (!string.IsNullOrEmpty(TagName))
			{
				_checkType = Types.First(s => s.Name.Equals(TagName));
			}
			_abilitiesOnStart = ActionsOnStart.Where(s => s != null && s is IActorAbility).Cast<IActorAbility>();
			_abilitiesOnSuccess = ActionsOnSuccess.Where(s => s != null && s is IActorAbility).Cast<IActorAbility>();

			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			StartTimer();
			if (ExecuteOnAwake)
			{
				Execute();
			}
		}

		[Button]
		public void Execute()
		{
			if (IsEnable)
			{
				return;
			}
			IsEnable = true;
			_abilitiesOnStart?.Exectute();
			Timer.TimedActions.AddAction(Check, CheckupDelay);
		}

		private void Check()
		{
			if (IsEnable)
			{
				var result = CheckActor(Actor);
				if (result)
				{
					IsEnable = false;
					_abilitiesOnSuccess?.Exectute();
				}
				else
				{
					Timer.TimedActions.AddAction(Check, UpdateTime);
				}
			}
		}

		private bool CheckActor(IActor actor)
		{
			return actor != null && (_checkType.Equals(null) || _entityManager.HasComponent(actor.ActorEntity, _checkType));
		}

		private string[] GetComponentTypes()
		{
			return Types
				.Where(s => s.GetInterfaces().Contains(typeof(IComponentData)))
				.Select(s => s.Name)
				.ToArray();
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		private bool MustBeSpawner(List<MonoBehaviour> behaviours)
		{
			return behaviours == null || behaviours.All(s => s is IActorSpawner);
		}
	}
}