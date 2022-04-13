using Crimson.Core.Common;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Assets.Crimson.Core.Common.Filters
{
	[Serializable]
	public struct ThresholdFilter : IFilter<IActor>
	{
		public bool NeedCheckPosition;

		[ShowIf(nameof(NeedCheckPosition))]
		public float DistanceTheshold;

		public bool NeedCheckRotation;

		[ShowIf(nameof(NeedCheckRotation))]
		[Range(0, 180)]
		public float RotationAngleThreshold;

		private Transform _target;

		public void SetTarget(Transform target)
		{
			_target = target;
		}

		public bool Filter(IActor target)
		{
			var result = NeedCheckPosition || NeedCheckRotation;

			if (!result)
			{
				return true;
			}

			if (NeedCheckPosition)
			{
				result &= Vector3.Distance(_target.position, target.GameObject.transform.position)
					<= DistanceTheshold;
			}

			if (NeedCheckRotation)
			{
				result &= Quaternion.Angle(_target.rotation, target.GameObject.transform.rotation)
					<= RotationAngleThreshold;
			}

			return result;
		}
	}
}