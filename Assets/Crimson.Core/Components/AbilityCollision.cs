using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
    [HideMonoScript]
    [NetworkSimObject]
    public class AbilityCollision : MonoBehaviour, IActorAbility
    {
        public IActor Actor { get; set; }

        [InfoBox("Only one Collider per Actor is supported! If no collider provided here, will use first")]
        public Collider useCollider;

        public List<CollisionAction> collisionActions = new List<CollisionAction>();

        public bool debugCollisions = false;
        [NetworkSimData]
        [HideInInspector]
        public List<Collider> ExistentCollisions = new List<Collider>();

        public List<Collider> OwnColliders
        {
            get => _ownCollidersSet ? _ownColliders : null;
            set
            {
                _ownCollidersSet = true;
                _ownColliders = value;
            }
        }

        private List<Collider> _ownColliders;
        private bool _ownCollidersSet;

        public List<Collider> SpawnerColliders
        {
            get => _spawnerCollidersSet ? _spawnerColliders : null;
            set
            {
                _spawnerCollidersSet = true;
                _spawnerColliders = value;
            }
        }

        private List<Collider> _spawnerColliders;
        private bool _spawnerCollidersSet = false;

        private static IEnumerable Tags()
        {
            return EditorUtils.GetEditorTags();
        }

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            if (useCollider == null && (useCollider = gameObject.GetComponent<Collider>()) == null && (useCollider = this.gameObject.GetComponentInChildren<Collider>()) == null)
            {
                Debug.LogError("[ABILITY COLLISION] Neither Collider is specified, nor it could be found!");
                return;
            }

            var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            float3 position = gameObject.transform.position;
            switch (useCollider)
            {
                case SphereCollider sphere:
                    sphere.ToWorldSpaceSphere(out var sphereCenter, out var sphereRadius);
                    dstManager.AddComponentData(entity, new ActorColliderData
                    {
                        ColliderType = ColliderType.Sphere,
                        SphereCenter = sphereCenter - position,
                        SphereRadius = sphereRadius,
                        initialTakeOff = true
                    });
                    break;
                case CapsuleCollider capsule:
                    capsule.ToWorldSpaceCapsule(out var capsuleStart, out var capsuleEnd, out var capsuleRadius);
                    dstManager.AddComponentData(entity, new ActorColliderData
                    {
                        ColliderType = ColliderType.Capsule,
                        CapsuleStart = capsuleStart - position,
                        CapsuleEnd = capsuleEnd - position,
                        CapsuleRadius = capsuleRadius,
                        initialTakeOff = true
                    });
                    break;
                case BoxCollider box:
                    box.ToWorldSpaceBox(out var boxCenter, out var boxHalfExtents, out var boxOrientation);
                    dstManager.AddComponentData(entity, new ActorColliderData
                    {
                        ColliderType = ColliderType.Box,
                        BoxCenter = boxCenter - position,
                        BoxHalfExtents = boxHalfExtents,
                        BoxOrientation = boxOrientation,
                        initialTakeOff = true
                    });
                    break;
            }

            Actor = actor;

            OwnColliders = this.gameObject.GetAllColliders();
        }

        public void Execute()
        {
        }
    }
    public struct ActorColliderData : IComponentData
    {
        public ColliderType ColliderType;
        public float3 SphereCenter;
        public float SphereRadius;
        public float3 CapsuleStart;
        public float3 CapsuleEnd;
        public float CapsuleRadius;
        public float3 BoxCenter;
        public float3 BoxHalfExtents;
        public quaternion BoxOrientation;
        public bool initialTakeOff;
        public float RayDistance;
        public float RayHeight;
    }

    public struct CollisionSendData : IComponentData
    {
        public int ActorStateId;
        public int HitStateId;
    }

    public struct CollisionReceiveData : IComponentData
    {
        public int ActorStateId;
        public int HitStateId;
    }
}