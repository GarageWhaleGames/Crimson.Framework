using Crimson.Core.Common;
using Sirenix.OdinInspector;
using System;

namespace Assets.Crimson.Core.Common.Filters
{
	[Serializable]
	public struct ComponentNameFilter : IFilter<IActor>
	{
		public bool use;

		[ShowIf(nameof(use))]
		public string componentName;

		public bool Filter(IActor target)
		{
			return !use || target.ComponentNames.Contains(componentName);
		}
	}
}