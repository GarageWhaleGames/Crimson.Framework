using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Tags
{
	public struct AnimationMeleeAttackTag : IComponentData
	{
		public int NameHash;

		public AnimationMeleeAttackTag(string name)
		{
			NameHash = Animator.StringToHash(name);
		}
	}
}