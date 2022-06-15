using DG.Tweening;
using UnityEngine;

namespace Assets.Crimson.Core.AI.AimLaserFX
{
	public class AimState : ILaserState
	{
		private readonly Transform _target;
		private readonly float _duration;
		private Tweener _rotateTween;

		public AimState(Transform target, float duration)
		{
			_target = target;
			_duration = duration;
		}

		public void Run()
		{
			_rotateTween = _target.DOLocalRotateQuaternion(Quaternion.identity, _duration);
		}

		public void Stop()
		{
			_rotateTween.Kill();
		}
	}
}