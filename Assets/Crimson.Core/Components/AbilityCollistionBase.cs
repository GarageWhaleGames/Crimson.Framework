using Crimson.Core.Common;
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
    public abstract class AbilityCollisionBase : MonoBehaviour, IActorAbility
    {
        public IActor Actor { get; set; }

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

        protected List<Collider> _ownColliders;
        protected bool _ownCollidersSet;

        public List<Collider> SpawnerColliders
        {
            get => _spawnerCollidersSet ? _spawnerColliders : null;
            set
            {
                _spawnerCollidersSet = true;
                _spawnerColliders = value;
            }
        }

        protected List<Collider> _spawnerColliders;
        protected bool _spawnerCollidersSet = false;

        protected static IEnumerable Tags()
        {
            return EditorUtils.GetEditorTags();
        }

        public abstract void AddComponentData(ref Entity entity, IActor actor);

        public abstract void Execute();
    }
}
