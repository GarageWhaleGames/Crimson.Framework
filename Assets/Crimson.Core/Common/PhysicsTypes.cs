using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crimson.Core.Common
{
	[Serializable]
	public struct CollisionAction
	{
		[InfoBox("Objects without layer set will always collide")]
		public LayerMask collisionLayerMask;

		[InfoBox("Filter by tags works in addition to Collision Layer Mask")]
		public bool useTagFilter;

		[ShowIf(nameof(useTagFilter))]
		[EnumToggleButtons]
		public TagFilterMode filterMode;

		[ShowIf(nameof(useTagFilter))]
		[ValueDropdown(nameof(Tags))]
		public List<string> filterTags;

		public VelocityFilter velocityFilter;

		public bool executeOnCollisionWithSpawner;

		public bool destroyAfterAction;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		[SerializeField]
		public List<MonoBehaviour> actions;

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}

		private static IEnumerable Tags()
		{
			return EditorUtils.GetEditorTags();
		}
	}

	[Serializable]
	public struct CollisionSettings
	{
		[InfoBox("Objects without layer set will always collide")]
		public LayerMask collisionLayerMask;

		[InfoBox("Filter by tags works in addition to Collision Layer Mask")]
		public bool useTagFilter;

		[ShowIf(nameof(useTagFilter))]
		[EnumToggleButtons]
		public TagFilterMode filterMode;

		[ShowIf(nameof(useTagFilter))]
		[ValueDropdown(nameof(Tags))]
		public List<string> filterTags;

		public VelocityFilter velocityFilter;

		public bool executeOnCollisionWithSpawner;

		public bool destroyAfterAction;

		private static IEnumerable Tags()
		{
			return EditorUtils.GetEditorTags();
		}
	}
}