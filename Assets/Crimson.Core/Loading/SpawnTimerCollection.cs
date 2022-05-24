using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Loading.SpawnDataTypes;
using Crimson.Core.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Crimson.Core.Loading
{
	public class SpawnTimerCollection : List<GameObject>
	{
		private int _currentItemIndex = 0;
		private TimerDelays _delays;
		private List<SpawnItemData> _spawnItems = new List<SpawnItemData>();
		private TimerComponent _timer;

		public SpawnTimerCollection()
		{
		}

		public SpawnTimerCollection(IEnumerable<GameObject> collection) : base(collection)
		{
		}

		public void SetItems(List<SpawnItemData> items)
		{
			_spawnItems = items;
		}

		public void SpawnWithOptions(TimerComponent timer, TimerDelays spawnDelays)
		{
			_currentItemIndex = 0;
			_delays = spawnDelays;
			_timer = timer;
			timer.TimedActions.AddAction(SpawnCurrentItem, _delays.StartupDelay);
		}

		private void SpawnCurrentItem()
		{
			if (_currentItemIndex >= _spawnItems.Count)
			{
				return;
			}

			Add(ActorSpawn.Spawn(_spawnItems[_currentItemIndex]));
			_currentItemIndex++;

			_timer.TimedActions.AddAction(SpawnCurrentItem, _delays.Delay * _currentItemIndex);
		}

		public void Spawn()
		{
			for (var i = 0; i < _spawnItems.Count; i++)
			{
				Add(ActorSpawn.Spawn(_spawnItems[i]));
			}
		}
	}
}