using Assets.Crimson.Core.AI.AimLaserFX;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe.States
{
	internal class PrepareShot : State
	{
		private AimSnipeBehaviour _source;
		private SearchingLaser _laser;
		private readonly DelayTimer _timer = new DelayTimer();

		public PrepareShot(AimSnipeBehaviour source) : base()
		{
			_source = source;
			_laser = source._searchingLaser;
			_timer.Start(_source.AttackDelay);
			_laser.StateType = LaserStateType.Aim;
		}

		public override void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			_source._target.rotation = Quaternion.LookRotation(_source.TargetDirection);
			if (_timer.IsExpire)
			{
				IsDone = true;
			}
		}
	}
}