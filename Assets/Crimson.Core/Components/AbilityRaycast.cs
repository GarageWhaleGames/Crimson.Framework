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

        public Transform ParentTransform => Actor == null ? gameObject.transform : Actor.Spawner.GameObject.transform;
        public Vector3 RaySource => ParentTransform.position + Vector3.up * Height;
        public Vector3 RayDirection => ParentTransform.forward * Distance;

        public float Distance = 1;
        public float Height = 1;
        public void AddComponentData(ref Entity entity, IActor actor)
        {
            var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            dstManager.AddComponentData(entity, new ActorColliderData
            {
                ColliderType = ColliderType.Raycast,
                RayDistance = Distance,
                RayHeight = Height,
                initialTakeOff = false
            }); ;

            Actor = actor;
        }

        public void Execute()
        {
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(RaySource, RayDirection);
        }
    }
}