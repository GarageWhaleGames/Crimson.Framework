using Sirenix.OdinInspector;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.AI.GeneralParams
{
	[Serializable, HideLabel, BoxGroup]
	public struct CurvePriority
	{
		[PropertyOrder(0), InfoBox("$GetXTooltip", nameof(_showTooltip))]
		public AnimationCurve Curve;

		[PropertyOrder(2)]
		public float MaxSample;

		[PropertyOrder(1), HorizontalGroup]
		public float MinSample;

		private bool _showTooltip;

		[HideInInspector, SerializeField] private string _xAxisTooltip;

		public CurvePriority(float minSample = 0, float maxSample = 100) : this()
		{
			Curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			MaxSample = maxSample;
			MinSample = minSample;
			XAxisTooltip = string.Empty;
		}

		public float SampleScale => MaxSample - MinSample;

		public string XAxisTooltip
		{
			get => _xAxisTooltip;
			set
			{
				_xAxisTooltip = value;
				_showTooltip = !string.IsNullOrEmpty(value);
			}
		}

		public float Evaluate(float value)
		{
			var sampleScale = SampleScale;
			var curveSample = math.clamp(
				(value - MinSample) / sampleScale, 0f, 1f);
			return Curve.Evaluate(curveSample);
		}

		private string GetXTooltip()
		{
			return string.IsNullOrEmpty(XAxisTooltip)
				? string.Empty
				: "Curve for behaviour priority based on " + XAxisTooltip;
		}
	}
}