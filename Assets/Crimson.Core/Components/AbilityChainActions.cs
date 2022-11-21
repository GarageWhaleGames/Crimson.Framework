using Assets.Crimson.Core.Common;
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
	[HideMonoScript]
	public class AbilityChainActions : TimerBaseBehaviour, IActorAbility, IHasComponentName
	{
		[SerializeField] public string componentName = "";
		public bool ExecuteOnStart = false;
		public AbilityChainSettings Settings = new AbilityChainSettings();
		private List<IActorAbility> _abilities;
		private int _currentAbility;
		private bool _isInfiniteLoop;
		private int _loopsCount;
		public IActor Actor { get; set; }

		public string ComponentName
		{
			get => componentName;
			set => componentName = value;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			if (componentName == null)
			{
				Debug.Log("Name is null");
			}
			_abilities = Settings.Actions.Cast<IActorAbility>().Where(s => s != null).ToList();
			StartTimer();
			if (ExecuteOnStart)
			{
				Execute();
			}
		}

		public void Execute()
		{
			if (_abilities.Count == 0)
			{
				return;
			}

			_loopsCount = Settings.LoopCount;
			_isInfiniteLoop = _loopsCount == 0;

			Timer.TimedActions.AddAction(ExecuteAbility, Settings.StartupDelay);
		}

		private void ExecuteAbility()
		{
			_abilities[_currentAbility].Execute();

			if (_currentAbility < _abilities.Count - 1)
			{
				_currentAbility++;
			}
			else
			{
				if (_isInfiniteLoop)
				{
					_currentAbility = 0;
				}

				if (_loopsCount > 0)
				{
					_loopsCount--;
					_currentAbility = 0;
				}

				if (_currentAbility != 0)
				{
					return;
				}
			}
			if (_isInfiniteLoop || _loopsCount != 0)
			{
				Timer.TimedActions.AddAction(ExecuteAbility, Settings.Delay);
			}
		}
	}
}