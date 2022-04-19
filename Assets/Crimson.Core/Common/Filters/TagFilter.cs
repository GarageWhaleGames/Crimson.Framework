using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Crimson.Core.Common.Filters
{
	[Serializable, HideLabel]
	public struct TagFilter : IFilter<IActor>, IFilter<Component>
	{
		[LabelText("Use tag filter")]
		public bool use;

		[ShowIf(nameof(use))]
		[EnumToggleButtons]
		public TagFilterMode filterMode;

		[ShowIf(nameof(use))]
		[ValueDropdown(nameof(Tags), IsUniqueList = true)]
		public List<string> filterTags;

		public bool Filter(IActor actor)
		{
			return Filter(actor.GameObject.transform);
		}

		private static IEnumerable Tags()
		{
			return EditorUtils.GetEditorTags();
		}

		public bool Filter(Component target)
		{
			if (!use)
			{
				return true;
			}

			var contains = filterTags.Contains(target.tag);

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
	}
}