using Crimson.Core.Common;
using Crimson.Core.Components;
using DG.Tweening;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.UI
{
	public class AbilityCanvasAlphaTween : MonoBehaviour, IActorAbility
	{
		public CanvasGroup CanvasGroup;
		public float FadeInTime = 2f;
		public bool InitValue = false;
		public IActor Actor { get; set; }
		public float EndValue => InitValue ? 0.0f : 1.0f;
		public float StartValue => InitValue ? 1.0f : 0.0f;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			Execute();
		}

		public void Execute()
		{
			CanvasGroup.alpha = StartValue;
			DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, EndValue, FadeInTime);
		}
	}
}