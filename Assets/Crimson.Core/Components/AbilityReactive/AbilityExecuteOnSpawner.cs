using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.AbilityReactive
{
	public class AbilityExecuteOnSpawner : MonoBehaviour, IActorAbility
	{
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbilityTarget!")]
		public MonoBehaviour[] Abilities;

		private ActorAbilityTargetList _abilities;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_abilities = new ActorAbilityTargetList(Abilities);
		}

		public void Execute()
		{
			_abilities.Execute(Actor.Spawner);
		}

		private bool MustBeAbility(MonoBehaviour[] a)
		{
			return (a is null) || (a.All(s => s is IActorAbilityTarget));
		}
	}
}