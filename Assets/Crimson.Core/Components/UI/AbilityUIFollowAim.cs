using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityUIFollowAim : MonoBehaviour, IActorAbility
	{
		public CanvasGroup CanvasGroup;
		public float HideRadius = .4f;
		public RectTransform Mark;

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