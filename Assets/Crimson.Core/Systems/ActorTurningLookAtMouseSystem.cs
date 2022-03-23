using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class ActorTurningLookAtMouseSystem:ComponentSystem
    {
        private EntityQuery _query;
        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                ComponentType.ReadOnly<ActorRotationLookAtMouseData>(),
                ComponentType.ReadOnly<Rigidbody>(),
                ComponentType.ReadOnly<PlayerInputData>(),
                ComponentType.Exclude<StopRotationData>());
        }
        protected override void OnUpdate()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;
            
            Entities.With(_query).ForEach(
                (Entity entity, Rigidbody rigidBody, ref PlayerInputData input, ref ActorRotationLookAtMouseData rotation) =>
                {
                    var mousePos = new Vector3(input.Mouse.x, input.Mouse.y, 0);
                    var camRay = mainCamera.ScreenPointToRay(mousePos);

                    if (!Physics.Raycast(camRay, out var floorHit, rotation.CamRayLen, rotation.Layer)) return;
                    
                    var position = rigidBody.gameObject.transform.position;
                    var playerToMouse = floorHit.point - new Vector3(position.x, position.y, position.z);
                    playerToMouse.y = 0f;
                    var newRot = Quaternion.LookRotation(playerToMouse);
                    rigidBody.MoveRotation(newRot);
                });
        }
    }
}