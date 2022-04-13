using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Crimson.Core.Common
{
	[Serializable]
	public class ActionsList
	{
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		[SerializeField]
		public List<MonoBehaviour> actions;

		public List<IActorAbility> AbilityCollection { get; private set; }
		public List<IActorAbilityTarget> AbilityTargetCollection { get; private set; }

		public void Init()
		{
			AbilityCollection = new List<IActorAbility>();
			AbilityTargetCollection = new List<IActorAbilityTarget>();

			foreach (var a in actions.Where(s => s != null))
			{
				switch (a)
				{
					case IActorAbilityTarget exchange:
						AbilityTargetCollection.Add(exchange);
						AbilityCollection.Add(exchange);
						break;

					case IActorAbility ability:
						AbilityCollection.Add(ability);
						break;
				}
			}
		}

		public void SetTarget(IActor actor)
		{
			for (var i = 0; i < AbilityTargetCollection.Count; i++)
			{
				AbilityTargetCollection[i].TargetActor = actor;
			}
		}

		public void Execute()
		{
			for (var i = 0; i < AbilityCollection.Count; i++)
			{
				AbilityCollection[i].Execute();
			}
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}
	}
}