using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.AI.AimLaserFX
{
	public class SearchingState : ILaserState
	{
		private Vector3 _targetRotatation;
		private Tweener _rotateTween;

		public SearchingState(Transform target, float angle, float duration)
		{
			Target = target;
			Angle = angle;
			_duration = duration;
		}

		public Transform Target { get; private set; }
		public float Angle { get; }

		private float _duration;
		private Tween _mainTween;

		public void Run()
		{
			_mainTween = DOVirtual.DelayedCall(_duration, DoRandomRotation);
			_mainTween.SetLoops(-1);
			DoRandomRotation();
		}

		private void DoRandomRotation()
		{
			_targetRotatation = GetRandomRotationEuler(Angle);
			_rotateTween = Target.DOLocalRotate(_targetRotatation, _duration);
		}

		public void Stop()
		{
			_mainTween.Kill();
			_rotateTween.Kill();
		}

		private Vector3 GetRandomRotationEuler(float angle)
		{
			var randomAngle = UnityEngine.Random.Range(-180, 180) * Mathf.Deg2Rad;
			var randomAngles = new float2()
			{
				x = angle * math.cos(randomAngle),
				y = angle * math.sin(randomAngle)
			};
			return new Vector3(randomAngles.x, randomAngles.y, 0);
		}
	}
}