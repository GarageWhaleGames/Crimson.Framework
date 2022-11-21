using Crimson.Core.Common;
using Crimson.Core.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Crimson.Core.Common
{
	public class ActorAbilityTargetList
	{
		private List<IActorAbilityTarget> _items;

		public ActorAbilityTargetList(MonoBehaviour[] components)
		{
			_items = components != null ? components.Where(s => s is IActorAbilityTarget).Cast<IActorAbilityTarget>().ToList() : new List<IActorAbilityTarget>();
		}

		public void Execute(IActor target)
		{
			for (var i = 0; i < _items.Count; i++)
			{
				_items[i].TargetActor = target;
				_items[i].Execute();
			}
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