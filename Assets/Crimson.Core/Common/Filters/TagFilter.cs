using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Crimson.Core.Common.Filters
{
	[Serializable]
	public struct TagFilter : IFilter<IActor>
	{
		public bool use;

		[ShowIf(nameof(use))]
		[EnumToggleButtons]
		public TagFilterMode filterMode;

		[ShowIf(nameof(use))]
		[ValueDropdown(nameof(Tags))]
		public List<string> filterTags;

		public bool Filter(IActor actor)
		{
			if (!use)
			{
				return true;
			}

			var contains = filterTags.Contains(actor.GameObject.tag);

			switch (filterMode)
			{
				case TagFilterMode.IncludeOnly:
					return contains;

				case TagFilterMode.Exclude:
					return !contains;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static IEnumerable Tags()
		{
			return EditorUtils.GetEditorTags();
		}
	}
}