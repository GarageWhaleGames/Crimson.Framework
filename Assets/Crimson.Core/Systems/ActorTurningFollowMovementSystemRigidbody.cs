using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class ActorTurningFollowMovementSystemRigidbody : ComponentSystem
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                ComponentType.ReadOnly<Transform>(),
                ComponentType.ReadOnly<ActorMovementData>(),
                ComponentType.ReadOnly<ActorRotationFollowMovementData>(),
                ComponentType.ReadOnly<Rigidbody>(),
                ComponentType.Exclude<StopRotationData>());
        }

        protected override void OnUpdate()
        {
            var dt = Time.fixedDeltaTime;
            
            Entities.With(_query).ForEach((Entity entity, Rigidbody rigidBody, ref ActorMovementData movement,
                ref ActorRotationFollowMovementData rotation) =>
            {
                if (rigidBody == null) return;
                
                var dir = new Vector3(movement.MovementCache.x, 0, movement.MovementCache.z);

                if (dir == Vector3.zero) return;
                
                var rot = rigidBody.rotation;
                var newRot = Quaternion.LookRotation(Vector3.Normalize(dir));
                if (newRot == rot) return;
                rigidBody.MoveRotation(Quaternion.Lerp(rot, newRot, dt * rotation.RotationSpeed));
            });
        }
    }
}