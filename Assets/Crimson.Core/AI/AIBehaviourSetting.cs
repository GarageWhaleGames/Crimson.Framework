using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crimson.Core.AI
{
	[Serializable]
	public class AIBehaviourSetting
	{
		public IActor Actor;

		[ShowIf("@ShowModes(behaviourType)")]
		[Space]
		[ValueDropdown("@GetModes(behaviourType)")]
		public string additionalMode = "";

		[HideIf("@GetOrCreateAI(behaviourType) == null")]
		[Range(0, 3)]
		public float basePriority = 1;

		[ValueDropdown(nameof(GetAIs))] public string behaviourType = "";

		[HideIf("@GetOrCreateAI(behaviourType) == null || HideCurve(behaviourType)")]
		public float curveMaxSample = 100;

		[HideIf("@GetOrCreateAI(behaviourType) == null || HideCurve(behaviourType)")]
		public float curveMinSample = 0;

		[ShowIf("@ShowActions(behaviourType)")]
		[Space]
		public int executeCustomInput = 1;

		[HideIf("@GetOrCreateAI(behaviourType) == null || HideCurve(behaviourType)")]
		[InfoBox("@GetCurveLabel(behaviourType)")]
		public AnimationCurve priorityCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[ShowIf("@ShowFilters(behaviourType)")]
		[Space]
		[EnumToggleButtons]
		public TagFilterMode targetFilterMode = TagFilterMode.IncludeOnly;

		[ShowIf("@ShowFilters(behaviourType)")]
		[ValueDropdown(nameof(Tags))]
		public List<string> targetFilterTags;

		private static Dictionary<string, Type> _aiTypes;
		private IAIBehaviour _behaviourInstance;
		public IAIBehaviour BehaviourInstance => GetOrCreateAI(behaviourType);

		private Dictionary<string, Type> AvaiableTypes
		{
			get
			{
				if (_aiTypes == null)
				{
					_aiTypes = AppDomain.CurrentDomain.GetAssemblies()
													  .SelectMany(s => s.GetTypes())
													  .Where(p => typeof(IAIBehaviour).IsAssignableFrom(p) && p.IsClass)
													  .GroupBy(s => s.Name.Split('.').Last())
													  .ToDictionary(s => s.Key, v => v.First());
				}

				return _aiTypes;
			}
		}

		private static IEnumerable<string> GetAIs()
		{
#if UNITY_EDITOR

			var l = new List<string> { string.Empty };
			l.AddRange(AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(IAIBehaviour).IsAssignableFrom(p) && p.IsClass)
				.Convert(a => a.ToString().Split('.').Last()));
			return l;
#else
			return null;
#endif
		}

		private static IEnumerable Tags()
		{
			return EditorUtils.GetEditorTags();
		}

		private string GetCurveLabel(string type)
		{
#if UNITY_EDITOR

			var b = GetOrCreateAI(type);
			return b == null || b.XAxis.Equals(string.Empty, StringComparison.Ordinal)
				? string.Empty
				: "Curve for behaviour priority based on " + b.XAxis;

#else
			return String.Empty;
#endif
		}

		private IEnumerable<string> GetModes(string type)
		{
#if UNITY_EDITOR

			var b = GetOrCreateAI(type);
			return b != null ? b.AdditionalModes : new string[0];
#else
			return null;
#endif
		}

		private IAIBehaviour GetOrCreateAI(string type)
		{
			if (type.Equals(string.Empty, StringComparison.Ordinal))
			{
				return null;
			}

			if (_behaviourInstance != null && _behaviourInstance.GetType().Name.Split('.').Last()
					.Equals(type, StringComparison.Ordinal))
			{
				return _behaviourInstance;
			}

			var t = AvaiableTypes[type];
			if (t == null)
			{
				Debug.LogError(
					$"[AI BEHAVIOUR ROOT] Cannot create {type} type behaviour class! Aborting AI composition");
				behaviourType = "";
				return null;
			}

			_behaviourInstance = Activator.CreateInstance(t) as IAIBehaviour;
			return _behaviourInstance;
		}

		private bool HideCurve(string type)
		{
#if UNITY_EDITOR

			var b = GetOrCreateAI(type);
			return !(b != null && b.NeedCurve);

#else
			return false;
#endif
		}

		private bool ShowActions(string type)
		{
#if UNITY_EDITOR

			var b = GetOrCreateAI(type);
			return b != null && b.NeedActions;
#else
			return false;
#endif
		}

		private bool ShowFilters(string type)
		{
#if UNITY_EDITOR

			var b = GetOrCreateAI(type);
			return b != null && b.NeedTarget;

#else
			return false;
#endif
		}

		private bool ShowModes(string type)
		{
#if UNITY_EDITOR

			var b = GetOrCreateAI(type);
			return b != null && b.AdditionalModes.Length > 0;
#else
			return false;
#endif
		}
	}
}