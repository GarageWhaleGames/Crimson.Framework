using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class ActorFollowMovementSystem : ComponentSystem
    {
        private EntityQuery _queryByInput, _queryDirectly;

        protected override void OnCreate()
        {
            _queryByInput = GetEntityQuery(
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<ActorFollowMovementData>(),
                ComponentType.ReadOnly<ActorMovementData>(),
                ComponentType.ReadOnly<MoveByInputData>(),
                ComponentType.Exclude<ActorNoFollowTargetMovementData>(),
                ComponentType.Exclude<StopMovementData>());

            _queryDirectly = GetEntityQuery(
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<ActorFollowMovementData>(),
                ComponentType.ReadOnly<MoveDirectlyData>(),
                ComponentType.Exclude<ActorNoFollowTargetMovementData>(),
                ComponentType.Exclude<StopMovementData>());
        }

        protected override void OnUpdate()
        {
            Entities.With(_queryByInput).ForEach(
                (Entity entity, AbilityFollowMovement follow, ref ActorMovementData movement) =>
                {
                    if (follow.Target == null)
                    {
                        PostUpdateCommands.AddComponent<ActorNoFollowTargetMovementData>(entity);
                        return;
                    }

                    if ((follow.findTargetProperties.strategy == ChooseTargetStrategy.Nearest) && (follow.findTargetProperties.maxDistanceThreshold > 0f) &&
                        (math.distancesq(follow.Target.position, follow.Actor.GameObject.transform.position) >
                         follow.findTargetProperties.maxDistanceThreshold * follow.findTargetProperties.maxDistanceThreshold))
                    {
                        if (!follow.continousFollow)
                        {
                            PostUpdateCommands.RemoveComponent<ActorFollowMovementData>(entity);
                        }
                        else
                        {
                            PostUpdateCommands.AddComponent<ActorNoFollowTargetMovementData>(entity);
                        }

                        return;
                    }

                    float3 delta = follow.Target.position - follow.gameObject.transform.position;
                    if (math.abs(delta.x) < Constants.FOLLOW_MOVEMENT_AXIS_THRESH) delta.x = 0f;
                    if (math.abs(delta.y) < Constants.FOLLOW_MOVEMENT_AXIS_THRESH) delta.y = 0f;
                    if (math.abs(delta.z) < Constants.FOLLOW_MOVEMENT_AXIS_THRESH) delta.z = 0f;
                    
                    movement.Input = delta;
                    Debug.Log($"{nameof(ActorFollowMovementSystem)} = {movement.Input} and {delta}");

                    if (!follow.continousFollow) PostUpdateCommands.RemoveComponent<ActorFollowMovementData>(entity);
                }
            );

            Entities.With(_queryDirectly).ForEach(
                (Entity entity, AbilityFollowMovement follow, ref MoveDirectlyData movement,
                    ref ActorFollowMovementData data) =>
                {
                    if (follow.Target == null)
                    {
                        PostUpdateCommands.AddComponent<ActorNoFollowTargetMovementData>(entity);
                        if (follow.hideIfNoTarget) follow.gameObject.SetActive(false);
                        return;
                    }
                    
                    if (follow.hideIfNoTarget) follow.gameObject.SetActive(true);
                    float3 targetPosition = follow.Target.position;
                    movement.Position = targetPosition;
                    if (follow.retainOffset) movement.Position -= data.Origin;
                }
            );
        }
    }
}