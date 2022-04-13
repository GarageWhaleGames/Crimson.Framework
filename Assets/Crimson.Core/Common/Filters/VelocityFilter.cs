using Assets.Crimson.Core.Enums;
using Sirenix.OdinInspector;
using System;

namespace Assets.Crimson.Core.Common.Filters
{
	[Serializable]
	public struct VelocityFilter
	{
		public bool Use;

		[ShowIf(nameof(Use))]
		public VelocityFilterMode Mode;

		[ShowIf(nameof(Use))]
		public float Velocity;

		public bool Filter(float value)
		{
			switch (Mode)
			{
				case VelocityFilterMode.MoreThan:
					return value >= Velocity;

				case VelocityFilterMode.LessThan:
					return value <= Velocity;

				default:
					return false;
			}
		}
	}
}