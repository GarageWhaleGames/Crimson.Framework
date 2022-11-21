using System;
using Unity.Entities;

namespace Assets.Crimson.Core.Common.Buffs
{
	public struct HealthPercentPerTimeBuffData : IBufferElementData
	{
		public float Duration;
		public float Percent;

		public HealthPercentPerTimeBuffData(float percent, float duration = 0)
		{
			Percent = Math.Clamp(percent, 0, 100);
			Duration = duration;
		}
	}
}