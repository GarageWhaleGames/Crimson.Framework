using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	[AddComponentMenu(nameof(AbilityGameObjectSetActive))]
	public class AbilityGameObjectSetActive : MonoBehaviour, IActorAbility
	{
		public bool State;

		public GameObject Target;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
			Target.SetActive(State);
		}
	}
}