using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components
{
    [HideMonoScript]
    [NetworkSimObject]
    public class AbilityRaycast : MonoBehaviour, IActorAbility
    {
        public IActor Actor { get; set; }

        public List<CollisionAction> collisionActions = new List<CollisionAction>();

        public bool debugCollisions = false;
        protected static IEnumerable Tags()
        {
            return EditorUtils.GetEditorTags();
        }

        public float RayDistance = 1;
        public float RayHeight = 1;
        public void AddComponentData(ref Entity entity, IActor actor)
        {
            var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            dstManager.AddComponentData(entity, new ActorColliderData
            {
                ColliderType = ColliderType.Raycast,
                Ray = new Ray(transform.position + Vector3.up * RayHeight, transform.forward),
                RayDistance = RayDistance,
                initialTakeOff = false
            });

            Actor = actor;
        }

        public void Execute()
        {
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up * RayHeight, transform.forward * RayDistance);
        }
    }
}