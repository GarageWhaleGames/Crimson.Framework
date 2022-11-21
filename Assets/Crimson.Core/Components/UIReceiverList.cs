using Assets.Crimson.Core.Common.UI;
using Crimson.Core.Common;
using Crimson.Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class UIReceiverList
	{
		[HideInInspector] public List<IActor> Items { get; private set; } = new List<IActor>();
		private Dictionary<string, FieldInfo> _fieldsInfo = new Dictionary<string, FieldInfo>();
		private object _parent;

		public void Init(object parent, Entity entity)
		{
			_fieldsInfo.Clear();
			_parent = parent;
			World.DefaultGameObjectInjectionWorld.EntityManager.AddComponent<UIReceiverListTag>(entity);

			foreach (var fieldInfo in parent.GetType().GetFields()
				.Where(field => field.GetCustomAttribute<CastToUI>(false) != null))
			{
				var attribute = fieldInfo.GetCustomAttribute<CastToUI>(false);
				_fieldsInfo.Add(attribute.FieldId, fieldInfo);
			}
		}

		public void UpdateUIData(string fieldName)
		{
			foreach (var receiver in Items)
			{
				((UIReceiver)receiver)?.UpdateUIElementsData(
					fieldName,
					_fieldsInfo[fieldName].GetValue(_parent));
			}
		}
	}
}