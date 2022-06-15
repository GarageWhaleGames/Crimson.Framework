using Assets.Crimson.Core.AI.AimLaserFX;
using Assets.Crimson.Core.AI.GeneralParams;
using Crimson.Core.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe.States
{
	internal class Aiming : State
	{
		private ExecutableCustomInput _input;
		private AimSnipeBehaviour _source;
		private SearchingLaser _laser;
		private Transform _transform;
		private DelayTimer _timer = new DelayTimer();
		private Transform _target;
		private Vector3 _lastPosition;
		private const float PositionThreshold = 1;

		public Aiming(AimSnipeBehaviour source)
		{
			_source = source;
			_laser = source._searchingLaser;
			_transform = _source._transform;
			_target = _source._target;
			_timer.Start(source.SearchTime);
			_laser.StateType = LaserStateType.Searching;
			_lastPosition = _target.position;
			_input = _source.ExecutableCustomInput;
		}

		public override void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			inputData.Move = float2.zero;
			inputData.CustomInput[_input.CustomInputIndex] = 0;
			if (math.distancesq(_target.position, _lastPosition) >= PositionThreshold)
			{
				_lastPosition = _target.position;
				_timer.Start(_source.SearchTime);
			}
			var direction = _source.TargetDirection;
			_transform.rotation = Quaternion.LookRotation(direction);
			if (_timer.IsExpire)
			{
				IsDone = true;
			}
			else
			{
				_laser.Angle = _source.CalculateMaxAngle();
			}
		}

		public override void Kill()
		{
			base.Kill();

			_laser.StateType = LaserStateType.Aim;
		}
	}
}