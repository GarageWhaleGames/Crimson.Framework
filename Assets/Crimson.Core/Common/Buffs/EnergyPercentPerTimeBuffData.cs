using System;
using Unity.Entities;

namespace Assets.Crimson.Core.Common.Buffs
{
	public struct EnergyPercentPerTimeBuffData : IBufferElementData
	{
		public float Duration;
		public float Percent;

		public EnergyPercentPerTimeBuffData(float percent, float duration = 0)
		{
			Percent = Math.Clamp(percent, 0, 100);
			Duration = duration;
		}
	}
}