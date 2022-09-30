using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class NavAgentAdapter : MonoBehaviour, IActorAbility
	{
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
		}

		public void Execute()
		{
		}
	}
}
