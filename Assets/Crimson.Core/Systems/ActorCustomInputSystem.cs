using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Components.AbilityReactive;
using Crimson.Core.Enums;
using Crimson.Core.Utils.LowLevel;
using System;
using Unity.Entities;

namespace Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class ActorCustomInputSystem : ComponentSystem
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                ComponentType.ReadOnly<PlayerInputData>(),
                ComponentType.ReadOnly<AbilityPlayerInput>(),
                ComponentType.Exclude<DeadActorTag>());
        }

        protected override void OnUpdate()
        {
            Entities.With(_query).ForEach(
                (Entity entity, AbilityPlayerInput mapping, ref PlayerInputData input) =>
                {
                    var playerInput = input;
                    foreach (var b in mapping.bindingsDict)
                    {
                        b.Value.ForEach(a =>
                        {
                            var reactiveParser = a as AbilityReactiveParser;
                            if (reactiveParser != null)
                            {
                                reactiveParser.Parse(b.Key, playerInput);
                            }
                            if (Math.Abs(playerInput.CustomInput[b.Key]) >= Constants.INPUT_THRESH)
                                a?.Execute();

                            if (mapping.inputSource != InputSource.UserInput) return;

                            PostUpdateCommands.AddComponent(entity, new NotifyButtonActionExecutedData
                            {
                                ButtonIndex = b.Key
                            });
                        });
                    }
                });
        }
    }
}