using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Assets.Crimson.Core.Editor
{
	public class SelectionPlacer : ScriptableWizard
	{
		public Vector3Int Limits;
		public Vector3 Spacing;
		private int3 _limits;
		private float3 _spacing;

		[MenuItem("Tools/" + nameof(SelectionPlacer))]
		private static void Open()
		{
			var instance = GetWindow<SelectionPlacer>();
			instance.Show();
		}

		private void OnWizardCreate()
		{
			var targets = Selection.gameObjects;
			if (targets == null || targets.Length == 0)
			{
				return;
			}
			_spacing = (float3)Spacing;
			var startPosition = targets[0].transform.position;
			_limits = new int3();
			for (var i = 0; i < targets.Length; i++)
			{
				var target = targets[i];
				target.transform.position = startPosition;
				Place(target.transform);
				if (Limits.x > 0 && _limits.x < Limits.x)
				{
					_limits.x++;
				}
				else if (Limits.y > 0 && _limits.y < Limits.x)
				{
					_limits.x = 0;
					_limits.y++;
				}
				else if (Limits.z > 0 && _limits.z < Limits.z)
				{
					_limits.x = 0;
					_limits.y = 0;
					_limits.z++;
				}
			}
		}

		private void Place(Transform target)
		{
			Vector3 spacing = _spacing * _limits;
			target.position += spacing;
		}
	}
}