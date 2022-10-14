using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	internal class AbilityComboAction : TimerBaseBehaviour, IActorAbility, IEnableable
	{
		[Header("Attack actions")]
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions = new List<MonoBehaviour>();

		public float ResetTime = 0.1f;

		private Entity _entity;
		private EntityManager _entityManager;
		private int _lastActionIndex = 0;

		public List<IActorAbility> AbilityCollection { get; private set; } = new List<IActorAbility>();
		public IActor Actor { get; set; }
		public bool IsEnable { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			StartTimer();
			Init();
		}

		public void Execute()
		{
			this.RemoveAction(ResetIndex);
			AbilityCollection[_lastActionIndex].Execute();
			_lastActionIndex++;
			_lastActionIndex %= actions.Count;
			this.AddAction(ResetIndex, ResetTime);
		}

		private void Init()
		{
			AbilityCollection = new List<IActorAbility>();

			foreach (var a in actions.Where(s => s != null))
			{
				switch (a)
				{
					case IActorAbility ability:
						AbilityCollection.Add(ability);
						break;
				}
			}
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		private void ResetIndex()
		{
			_lastActionIndex = 0;
		}
	}
}