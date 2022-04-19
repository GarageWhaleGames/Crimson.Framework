using Crimson.Core.Components;
using System.Collections.Generic;

namespace Crimson.Core.Utils
{
	public static class AIUtils
	{
		public static bool ActionPossible(this List<IActorAbility> abilities)
		{
			var possible = false;

			foreach (var a in abilities)
			{
				if (a is IEnableable enableable)
				{
					possible |= enableable.IsEnable;
				}
				else
				{
					possible = true;
				}

				return possible;
			}

			return false;
		}
	}
}