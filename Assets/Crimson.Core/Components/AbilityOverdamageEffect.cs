using Assets.Crimson.Core.Common.ComponentDatas;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityOverdamageEffect : MonoBehaviour, IActorAbility, IHasComponentName
	{
		[SerializeField]
		public string componentName = "";

		public bool NeedDestroyWhenTriggered;
		public float TriggerDamage = 100;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions = new List<MonoBehaviour>();

		private EntityManager _dstManager;
		private Entity _entity;
		private IEnumerable<IActorAbility> actorAbilities;
		public IActor Actor { get; set; }
		public string ComponentName { get => componentName; set => componentName = value; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_entity = entity;
			actorAbilities = actions.Cast<IActorAbility>();
		}

		public void Execute()
		{
			if (!_dstManager.HasComponent<OverdamageData>(_entity))
			{
				return;
			}

			var overdamageData = _dstManager.GetComponentData<OverdamageData>(_entity);
			if (overdamageData.Damage >= TriggerDamage)
			{
				ExecuteActions();

				if (NeedDestroyWhenTriggered)
				{
					if (_dstManager.HasComponent<AbilityActorPlayer>(_entity))
					{
						var actorPlayer = _dstManager.GetComponentObject<AbilityActorPlayer>(_entity);
						actorPlayer.StartDeathTimer();
					}
				}
			}
		}

		private void ExecuteActions()
		{
			foreach (var ability in actorAbilities)
			{
				ability?.Execute();
			}
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}
	}
}