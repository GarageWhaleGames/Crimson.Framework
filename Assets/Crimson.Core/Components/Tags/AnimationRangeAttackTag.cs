using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Tags
{
	public struct AnimationRangeAttackTag : IComponentData
	{
		public int NameHash;

		public AnimationRangeAttackTag(string name)
		{
			NameHash = Animator.StringToHash(name);
		}
	}
}