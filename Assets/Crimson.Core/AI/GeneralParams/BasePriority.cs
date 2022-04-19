using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Assets.Crimson.Core.AI.GeneralParams
{
	[Serializable, HideLabel]
	public struct BasePriority
	{
		[Range(0, 3), LabelText(nameof(BasePriority))]
		public float Value;

		public BasePriority(float defaultValue = 1)
		{
			Value = defaultValue;
		}
	}
}