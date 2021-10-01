using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using Unity.Collections;

namespace Crimson.Core.Systems
{
    public class CooldownUiSystem : ComponentSystem
    {
        private EntityQuery _cooldownQuery;

        protected override void OnCreate()
        {
            _cooldownQuery = GetEntityQuery(ComponentType.ReadOnly<AbilityActorPlayer>(),
                ComponentType.ReadWrite<BindedActionsCooldownData>());
        }

        protected override void OnUpdate()
        {
            Entities.With(_cooldownQuery).ForEach(
                (Entity entity, AbilityActorPlayer abilityActorPlayer,
                    ref BindedActionsCooldownData cooldownData) =>
                {
                    if (cooldownData.OnCooldownBindingIndexes.Length == 0 &&
                        cooldownData.ReadyToUseBindingIndexes.Length == 0) return;

                    var data = cooldownData;

                    var cooldownable = abilityActorPlayer.Actor.Abilities
                        .Where(p => p is ICooldownable && p is IBindable && p is ITimer)
                        .Where(c => data.OnCooldownBindingIndexes.Contains(((IBindable) c).BindingIndex) ||
                                    data.ReadyToUseBindingIndexes.Contains(((IBindable) c).BindingIndex))
                        .ToList();

                    foreach (var ability in cooldownable)
                    {
                        for (var i = 0; i < cooldownData.OnCooldownBindingIndexes.Length; i++)
                        {
                            if (((IBindable) ability).BindingIndex != cooldownData.OnCooldownBindingIndexes[i])
                                continue;

                            var currentCooldownData = cooldownData;
                            var index = i;

                            abilityActorPlayer.UIReceiverList.ForEach(r =>
                            {
                                var currentUiElement = ((UIReceiver) r).customButtons.FirstOrDefault(b =>
                                    b.bindingIndex == currentCooldownData.OnCooldownBindingIndexes[index]);
                                if (currentUiElement == null) return;

                                var currentTimer = ((ITimer) ability).Timer.TimedActions
                                    .Where(t => ReferenceEquals(t.Act.Target, ability)).OrderByDescending(a => a.Delay)
                                    .FirstOrDefault();

                                if (currentTimer.Equals(new TimerAction())) return;
                                var cooldownValue = currentUiElement.reverseProgressBarDirection
                                    ? currentTimer.Delay / ((ICooldownable) ability).CooldownTime
                                    : 1 - currentTimer.Delay / ((ICooldownable) ability).CooldownTime;
                                currentUiElement.SetCooldownProgressBar(cooldownValue);
                                currentUiElement.SetCooldownText((int)(currentTimer.Delay +1));
                            });
                        }

                        var indexesToRemove = new List<int>();

                        for (var i = 0; i < cooldownData.ReadyToUseBindingIndexes.Length; i++)
                        {
                            if (((IBindable) ability).BindingIndex != cooldownData.ReadyToUseBindingIndexes[i])
                                continue;

                            var currentCooldownData = cooldownData;
                            var index = i;

                            abilityActorPlayer.UIReceiverList.ForEach(r =>
                            {
                                var currentUiElement = ((UIReceiver) r).customButtons.FirstOrDefault(b =>
                                    b.bindingIndex == currentCooldownData.ReadyToUseBindingIndexes[index]);
                                if (currentUiElement == null) return;

                                currentUiElement.SetCooldownProgressBar(currentUiElement.reverseProgressBarDirection?0f:1f);
                                currentUiElement.HideCoolDownText();
                                indexesToRemove.Add(currentCooldownData.ReadyToUseBindingIndexes[index]);
                            });

                            foreach (var idx in indexesToRemove)
                            {
                                currentCooldownData.ReadyToUseBindingIndexes.Remove(idx);
                            }

                            cooldownData = currentCooldownData;
                        }

                        var cooldown = cooldownData;

                        foreach (var idxToRemove in from idx in indexesToRemove
                            where cooldown.OnCooldownBindingIndexes.Contains(idx)
                            select cooldown.OnCooldownBindingIndexes.IndexOf(idx))
                        {
                            cooldownData.OnCooldownBindingIndexes.RemoveAt(idxToRemove);
                        }

                        PostUpdateCommands.SetComponent(entity, cooldownData);
                    }
                });
        }
    }
}