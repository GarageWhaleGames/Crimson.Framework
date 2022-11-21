using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.UI
{
	[HideMonoScript]
	public class AbilityUIFollowMouse : MonoBehaviour, IActorAbility
	{
		public CanvasGroup CanvasGroup;
		public float HideRadius = .4f;
		public RectTransform TargetRect;
		public IActor Actor { get; set; }
		public Actor Owner { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			Owner = (Actor)Actor.Spawner;
		}

		public void Execute()
		{
		}
	}
}