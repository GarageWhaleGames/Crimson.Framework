using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.FX.ForCamera
{
	public abstract class AbilityCameraFX : MonoBehaviour, IActorAbility
	{
		public IActor Actor { get; set; }

		public virtual void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public abstract void Execute();
	}
}