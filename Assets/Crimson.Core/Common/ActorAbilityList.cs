using Crimson.Core.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Crimson.Core.Common
{
	public class ActorAbilityList
	{
		private List<IActorAbility> _items;

		public ActorAbilityList(MonoBehaviour[] components)
		{
			_items = components != null ? components.Where(s => s is IActorAbility).Cast<IActorAbility>().ToList() : new List<IActorAbility>();
		}

		public void Execute()
		{
			for (var i = 0; i < _items.Count; i++)
			{
				_items[i].Execute();
			}
		}
	}
}