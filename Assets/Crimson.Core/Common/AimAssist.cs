﻿using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Crimson.Core.Common
{
	public struct AimData : IComponentData
	{
		public float3 LockedPosition;
		public Entity Target;
	}
}