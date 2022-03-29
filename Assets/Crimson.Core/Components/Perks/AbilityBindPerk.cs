using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Perks
{
	[HideMonoScript]
	public class AbilityBindPerk : MonoBehaviour, IActorAbility
	{
		[InfoBox(
			"Bind Abilities calls to Custom Inputs, indexes 0..9 represent keyboard keys of 0..9.\n" +
			"Further bindings are as set in User Input")]
		public InputBinding binding;

		public bool destroyAfterActions = true;
		public bool removeSameIndexBindings;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;

			var abilityPlayerInput = actor.Abilities.FirstOrDefault(a => a is AbilityPlayerInput) as AbilityPlayerInput;

			if (abilityPlayerInput == null)
			{
				return;
			}

			if (removeSameIndexBindings)
			{
				var existingBindings = abilityPlayerInput.GetInputBindings(binding.ID);

				if (existingBindings.Any() )
				{
					foreach (var binding in existingBindings)
					{
						binding.actions.ForEach(a =>
						{
							if (a is IPerkAbility ability)
							{
								ability.Remove();
							}
							else
							{
								Destroy(a);
							}
						});
					}

					abilityPlayerInput.RemoveBindingByID(binding.ID);
				}
			}			

			abilityPlayerInput.AddBinding(binding);

			if (destroyAfterActions)
			{
				Destroy(this);
			}
		}

		public void Execute()
		{
		}

		private void OnDestroy()
		{
			if (Actor != null && Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Remove(this);
			}
		}
	}
}