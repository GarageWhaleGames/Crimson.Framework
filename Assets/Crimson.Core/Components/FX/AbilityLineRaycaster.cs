using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.FX
{
	public class AbilityLineRaycaster : MonoBehaviour, IActorAbility
	{
		public float MaxLength = 100;
		public LineRenderer Renderer;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
		}
	}
}