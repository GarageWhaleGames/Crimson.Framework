using Sirenix.OdinInspector;
using System;

namespace Assets.Crimson.Core.AI.GeneralParams
{
	[Serializable, HideLabel]
	public struct DistanceLimitation
	{
		[ValidateInput(nameof(ValidateDistance), "Incorrect distance. Need more than 0")]
		public float MaxDistance;

		private bool ValidateDistance(float value)
		{
			return value >= 0 && value <= float.MaxValue;
		}
	}
}