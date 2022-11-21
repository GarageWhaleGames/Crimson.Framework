using Crimson.Core.Common;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.FX.ForCamera
{
	public class AbilityCameraShake : AbilityCameraFX
	{
		[TitleGroup("Shake settings")] public float Duration = 1;
		public bool ExecuteOnAwake;
		[TitleGroup("Shake settings")] public float Strength = 90;
		private Transform _target;

		public override void AddComponentData(ref Entity entity, IActor actor)
		{
			if (ExecuteOnAwake)
			{
				Execute();
			}
		}

		public override void Execute()
		{
			_target = Camera.main.transform;
			PositionShake();
		}

		[Button]
		private void PositionShake()
		{
			_target.DOShakePosition(Duration, Strength);
		}

		[Button]
		private void RotationShake()
		{
			_target.DOShakeRotation(Duration, Strength);
		}
	}
}