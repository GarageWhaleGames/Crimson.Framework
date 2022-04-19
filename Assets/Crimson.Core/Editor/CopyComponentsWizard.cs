using UnityEditor;
using UnityEngine;

namespace Assets.Crimson.Core.Editor
{
	public class CopyComponentsWizard : ScriptableWizard
	{
		public GameObject[] From;
		public GameObject To;

		[MenuItem("Tools/" + nameof(CopyComponentsWizard))]
		public static void ShowWizard()
		{
			var wizard = CreateInstance<CopyComponentsWizard>();
			wizard.Show();
		}

		private void Copy(MonoBehaviour[] monoBehaviours)
		{
			for (var i = 0; i < monoBehaviours.Length; i++)
			{
				var item = monoBehaviours[i];
				CopyComponent(item, To);
			}
		}

		private Component CopyComponent(Component original, GameObject destination)
		{
			System.Type type = original.GetType();
			Component copy = destination.AddComponent(type);
			// Copied fields can be restricted with BindingFlags
			System.Reflection.FieldInfo[] fields = type.GetFields();
			foreach (System.Reflection.FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(original));
			}
			return copy;
		}

		private void OnWizardCreate()
		{
			for (var i = 0; i < From.Length; i++)
			{
				if (From[i] == null)
				{
					continue;
				}
				Copy(From[i].GetComponentsInChildren<MonoBehaviour>());
			}
		}
	}
}