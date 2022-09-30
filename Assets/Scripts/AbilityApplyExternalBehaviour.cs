using BehaviorDesigner.Runtime;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts
{
	public class AbilityApplyExternalBehaviour : MonoBehaviour, IActorAbility
	{
		public ExternalBehaviorTree _externalBehavior;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			var behaviour = gameObject.GetComponent<BehaviorTree>();
			behaviour.ExternalBehavior = _externalBehavior;
			Destroy(this);
		}

		public void Execute()
		{
		}
	}
}