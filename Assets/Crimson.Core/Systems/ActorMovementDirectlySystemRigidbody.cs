using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class ActorMovementDirectlySystemRigidbody : ComponentSystem
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<MoveDirectlyData>(),
                ComponentType.Exclude<ActorNoFollowTargetMovementData>(),
                ComponentType.ReadOnly<Rigidbody>(),
                ComponentType.Exclude<StopMovementData>(),
                ComponentType.Exclude<DeadActorTag>());
        }

        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;

            Entities.With(_query).ForEach(
                (Entity entity, Rigidbody transform, ref MoveDirectlyData movement) =>
                {
                    if (transform == null) return;

                    float3 position = transform.position;
                    float3 delta = movement.Position - position;

                    if (movement.Speed > 0 && math.lengthsq(delta) > movement.Speed * movement.Speed * dt * dt)
                    {
                        delta = MathUtils.ClampMagnitude(delta, movement.Speed * dt);
                    }

                    if (math.lengthsq(delta) < Constants.FOLLOW_MOVEMENT_SQDIST_THRESH) return;
                    transform.position = position + delta;
                });
        }
    }
}