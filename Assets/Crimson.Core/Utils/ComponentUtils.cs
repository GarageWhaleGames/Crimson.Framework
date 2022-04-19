using Crimson.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using Object = System.Object;

namespace Crimson.Core.Utils
{
	public static class ComponentUtils
	{
		private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance |
								   BindingFlags.Default | BindingFlags.DeclaredOnly;

		public static Component CopyComponent(this GameObject go, Component sample)
		{
			return go.AddComponent(sample.GetType()).GetCopyOf(sample);
		}

		public static List<Component> CopyComponentsWithLinks(this GameObject dest, params Component[] components)
		{
			var newComponents = new List<Component>();
			var copies = new Dictionary<Component, Component>();

			Component component;
			for (var i = 0; i < components.Length; i++)
			{
				component = components[i];
				var newComponent = CopyComponent(dest, component);
				copies.Add(component, newComponent);
				newComponents.Add(newComponent);
			}

			foreach (var newComponent in newComponents)
			{
				UpdateComponentFields(newComponent, copies);
			}

			return newComponents;
		}

		public static Entity Damage(this Entity targetEntity, Entity ownerEntity, float damage)
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			var damageInstance = dstManager.CreateEntity();
			//TODO: Figure out, why this crashes builds
			//dstManager.SetName(damageInstance, "damage instance");
			dstManager.AddComponentData(damageInstance, new DamageData
			{
				DamageValue = damage,
				AbilityOwnerEntity = ownerEntity,
				TargetEntity = targetEntity
			});

			return damageInstance;
		}

		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			if (type != other.GetType()) return null; // type mis-match
			PropertyInfo[] pInfos = type.GetProperties(Flags);
			foreach (var pInfo in pInfos)
			{
				if (!pInfo.CanWrite) continue;

				try
				{
					pInfo.SetValue(comp, pInfo.GetValue(other, null), null);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[COMPONENT REPLICATOR] Error while copying properties. {comp.GetType().Name}");
					Debug.LogException(ex);
				} // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
			}

			FieldInfo[] fInfos = type.GetFields(Flags);
			foreach (var fInfo in fInfos)
			{
				fInfo.SetValue(comp, fInfo.GetValue(other));
			}

			return comp as T;
		}

		public static List<FieldInfo> GetFieldsWithAttributeInfo<T>(this Component component) where T : Attribute
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
									   BindingFlags.Default | BindingFlags.DeclaredOnly;

			var fields = component.GetType().GetFields(flags).ToList();

			return (from field in fields
					let attrs = field.GetCustomAttributes(true)
					from attr in attrs
					where attr is T
					select field).ToList();
		}

		public static bool IsNullOrEmpty(this IEnumerable source)
		{
			if (source != null)
			{
				foreach (object obj in source)
				{
					return false;
				}
			}

			return true;
		}

		private static Object UpdateComponentFields(Object obj, Dictionary<Component, Component> copies)
		{
			var type = obj.GetType();

			var fInfos = type.GetFields(Flags);

			foreach (var fInfo in fInfos)
			{
				var tempObj = fInfo.GetValue(obj);
				var resultObj = UpdateObject(tempObj, copies);

				fInfo.SetValue(obj, resultObj);
			}

			return obj;
		}

		private static Object UpdateObject(Object obj, Dictionary<Component, Component> copies)
		{
			var newObj = obj;

			if (newObj is MonoBehaviour)
			{
				var tempComponent = (Component)newObj;

				if (copies.ContainsKey(tempComponent))
				{
					return copies[tempComponent] as MonoBehaviour;
				}
			}
			else if (newObj != null && (newObj.GetType().IsClass || !newObj.GetType().IsPrimitive))
			{
				if (newObj is IEnumerable enumerable)
				{
					if (enumerable.IsNullOrEmpty()) return enumerable;

					if (enumerable is IList tempList)
					{
						if (tempList.Count == 0) return enumerable;

						IList outEnumerable;
						if (enumerable.GetType().IsArray)
						{
							outEnumerable = tempList is MonoBehaviour[]? new MonoBehaviour[tempList.Count]
								: (IList)Array.CreateInstance(tempList.GetType().GenericTypeArguments[0], tempList.Count);
						}
						else
						{
							outEnumerable = tempList is List<MonoBehaviour>
								? new List<MonoBehaviour>()
								: (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(tempList.GetType().GenericTypeArguments[0]));
						}

						for (var i = 0; i < tempList.Count; i++)
						{
							var outObj = UpdateObject(tempList[i], copies);

							if (outEnumerable.IsFixedSize)
							{
								outEnumerable[i] = outObj;
							}
							else
							{
								outEnumerable.Add(outObj);
							}
						}

						return outEnumerable;
					}

					foreach (var o in enumerable)
					{
						UpdateComponentFields(o, copies);
					}

					return enumerable;
				}
				else
				{
					newObj = UpdateComponentFields(newObj, copies);
					return newObj;
				}
			}

			return newObj;
		}
	}
}