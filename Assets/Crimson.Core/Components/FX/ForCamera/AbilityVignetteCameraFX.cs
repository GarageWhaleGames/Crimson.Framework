using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Crimson.Core.Components.FX.ForCamera
{
	public class AbilityVignetteCameraFX : AbilityCameraFX
	{
		public CanvasGroup Target;
		public float Duration = .33f;

		private void Awake()
		{
			Target.alpha = 0;
		}

		[Button]
		public override void Execute()
		{
			var sequence = DOTween.Sequence();
			sequence.Append(DOVirtual.Float(0, 1, Duration / 2, ChangeAlpha));
			sequence.Append(DOVirtual.Float(1, 0, Duration / 2, ChangeAlpha));
			sequence.Play();
		}

		private void ChangeAlpha(float value)
		{
			Target.alpha = value;
		}
	}
}