using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Crimson.Core.Components.FX.ForCamera
{
	public class AbilityCameraShake : AbilityCameraFX
	{
		public Transform Target;

		public float Duration = 1;
		public float Strength = 90;

		public override void Execute()
		{
			PositionShake();
		}

		[Button]
		private void RotationShake()
		{
			Target.DOShakeRotation(Duration, Strength);
		}

		[Button]
		private void PositionShake()
		{
			Target.DOShakePosition(Duration, Strength);
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			if (Target == null)
			{
				Target = transform;
			}
		}

#endif
	}
}