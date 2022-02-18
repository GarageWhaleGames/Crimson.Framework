using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Common
{
	[HideMonoScript]
	public class Perk : Actor
	{
		[HideInInspector] public List<IPerkAbility> PerksToApply = new List<IPerkAbility>();

		public override void HandleAbilities(Entity entity)
		{
			Abilities = GetComponents<IActorAbility>().ToList();

			foreach (var a in GetComponents<IActorAbility>().ToList())
			{
				if (a is IPerkAbility p)
				{
					PerksToApply.Add(p);
				}
				else
				{
					Abilities.Add(a);
				}
			}

			foreach (var ability in Abilities)
			{
				ability.AddComponentData(ref entity, this);
				if (ability is IComponentName componentName && !componentName.ComponentName.Equals(string.Empty))
				{
					ComponentNames.Add(componentName.ComponentName);
				}
			}
		}

		public override void PostConvert()
		{
			foreach (var p in PerksToApply)
			{
				p.Apply(Spawner);
			}

			base.PostConvert();
		}
	}
}