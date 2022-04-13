using System.Collections.Generic;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
    public class AIEvaluateSystem : ComponentSystem
    {
        private EntityQuery _queryAI;
        private EntityQuery _queryTargets;

        private readonly List<Transform> _targets = new List<Transform>();

        protected override void OnCreate()
        {
            _queryAI = GetEntityQuery(
                ComponentType.ReadWrite<EvaluateAITag>(),
                ComponentType.ReadOnly<AIInputData>(),
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<AbilityAIInput>(),
                ComponentType.Exclude<DeadActorTag>(),
                ComponentType.Exclude<DestructionPendingTag>(),
                ComponentType.Exclude<SetupAITag>(),
                ComponentType.Exclude<NetworkInputData>());
            _queryTargets = GetEntityQuery(
                ComponentType.ReadOnly<Transform>(),
                ComponentType.Exclude<DeadActorTag>(),
                ComponentType.Exclude<DestructionPendingTag>(),
                ComponentType.Exclude<UIReceiverTag>());
        }

        protected override void OnUpdate()
        {
            _targets.Clear();
            Entities.With(_queryAI).ForEach((Entity entity, AbilityAIInput ai) =>
                {
                    if (_targets.Count == 0)
                        Entities.With(_queryTargets).ForEach((Entity e, Transform transform) =>
                        {
                            _targets.Add(transform);
                        });
                    
                    var bestPriority = float.MinValue;
                    AIBehaviourSetting bestBehaviour = null;

                    foreach (var behaviour in ai.behaviours)
                    {
                        var b = behaviour.BehaviourInstance;
                        if (b == null)
                        {
                            Debug.LogError("[AI ABILITY] Could not find or create AI Behaviour instance!");
                            return;
                        }

                        var score = b.Evaluate(entity, behaviour, ai,  _targets);

                        if (score <= bestPriority) continue;

                        bestPriority = score;
                        bestBehaviour = behaviour;
                    }
                    
                    ai.activeBehaviour = bestBehaviour;
                    ai.activeBehaviourPriority = bestPriority;
                    
                    World.EntityManager.RemoveComponent<EvaluateAITag>(entity);
                    World.EntityManager.AddComponent<SetupAITag>(entity);
                }
            );
            
            
        }
    }
}