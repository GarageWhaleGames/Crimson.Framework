using System.Linq;
using Crimson.Core.Common;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components
{
    [HideMonoScript]
    public class AbilityAddActionsToPlayerInput : MonoBehaviour, IActorAbility
    {
        [InfoBox(
            "Bind Abilities calls to Custom Inputs, indexes 0..9 represent keyboard keys of 0..9.\n" +
            "Further bindings are as set in User Input")]
        public CustomBinding customBinding;
        public bool destroyAfterActions = true;
        public bool removeSameIndexBindings;
        
        public IActor Actor { get; set; }
        
        
        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;

            var abilityPlayerInput = actor.Abilities.FirstOrDefault(a => a is AbilityPlayerInput) as AbilityPlayerInput;

            if (abilityPlayerInput == null) return;
            
            var existingBindings = abilityPlayerInput.customBindings
                .Where(binding => binding.index == customBinding.index).ToList();

            if (existingBindings.Any() && removeSameIndexBindings)
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
                
                abilityPlayerInput.RemoveCustomBinding(customBinding.index);
            }
            
            abilityPlayerInput.AddCustomBinding(customBinding); 
            
            if (destroyAfterActions) Destroy(this);
        }

        public void Execute()
        {
        }

        private void OnDestroy()
        {
            if(Actor != null && Actor.Abilities.Contains(this))
            {
                Actor.Abilities.Remove(this);
            }
        }
    }
}