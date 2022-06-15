using Assets.Crimson.Core.AI.GeneralParams;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe.States
{
	internal class Shot : State
	{
		private AimSnipeBehaviour _source;
		private ExecutableCustomInput _input;
		private float _delayTime = 2.0f;
		private readonly DelayTimer _timer = new DelayTimer();

		public Shot(AimSnipeBehaviour source) : base()
		{
			_source = source;
			_input = _source.ExecutableCustomInput;
		}

		public override void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			_source._target.rotation = Quaternion.LookRotation(_source.TargetDirection);
			if (_timer.IsStopped)
			{
				_timer.Start(_delayTime);
				inputData.CustomInput[_input.CustomInputIndex] = 1.0f;
			}
			else if (_timer.IsExpire)
			{
				IsDone = true;
			}
		}
	}
}