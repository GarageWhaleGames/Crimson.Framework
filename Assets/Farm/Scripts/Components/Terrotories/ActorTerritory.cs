using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Farm.Scripts.Components.Terrotories
{
    public class ActorTerritory : MonoBehaviour, IActor, IComponentName, IConvertGameObjectToEntity
    {
        [Space] [SerializeField] public string componentName = "";

        [Space]
        [ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
        public List<MonoBehaviour> ExecuteOnSpawn = new List<MonoBehaviour>();

        private List<IActorAbility> _abilities = new List<IActorAbility>();

        private int _actorId;

        private int _actorStateId;

        private List<IPerkAbility> _appliedPerks = new List<IPerkAbility>();

        public List<IActorAbility> Abilities
        {
            get
            {
                _abilities.RemoveAll(a => a.Equals(null));
                return _abilities;
            }
            set => _abilities = value;
        }

        public Entity ActorEntity { get; set; }

        public int ActorId
        {
            get
            {
                if (_actorId == 0)
                {
                    _actorId = Spawner?.ActorId ?? 0;
                }

                return _actorId;
            }
            set => _actorId = value;
        }

        public int ActorStateId
        {
            get
            {
                if (_actorStateId != 0) return _actorStateId;
                if (ActorId == 0)
                {
                    return 0;
                }
                else
                {
                    _actorStateId = Spawner != null ?
                        Spawner.ActorStateId + Spawner.ChildrenSpawned + 1 :
                        ActorId * 1000;
                    return _actorStateId;
                }
            }
        }

        public List<IPerkAbility> AppliedPerks
        {
            get
            {
                _appliedPerks.RemoveAll(a => a.Equals(null));
                return _appliedPerks;
            }
            set => _appliedPerks = value;
        }

        public ushort ChildrenSpawned { get; set; }

        public string ComponentName
        {
            get => componentName;
            set => componentName = value;
        }

        public List<string> ComponentNames { get; } = new List<string>();
        public GameObject GameObject => this != null && gameObject != null ? gameObject : null;
        public IActor Owner { get; set; }
        public IActor Spawner { get; set; }
        public EntityManager WorldEntityManager { get; set; }

        public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            ActorEntity = entity;
            WorldEntityManager = dstManager;
            WorldEntityManager.AddComponent<NetworkSyncReceive>(ActorEntity);
            if (!ComponentName.Equals(string.Empty)) ComponentNames.Add(ComponentName);

            PostConvert();

            HandleAbilities(entity);
        }

        public virtual void HandleAbilities(Entity entity)
        {
            Abilities = GetComponents<IActorAbility>().ToList();

            foreach (var ability in Abilities)
            {
                ability.AddComponentData(ref entity, this);
                if (ability is IComponentName compName && !compName.ComponentName.Equals(string.Empty))
                {
                    ComponentNames.Add(compName.ComponentName);
                }
            }
        }

        public void PerformSpawnActions()
        {
            foreach (var component in ExecuteOnSpawn)
            {
                if (!(component is IActorAbility))
                {
                    Debug.LogError($"[ACTOR ABILITY EXECUTION] \"{component.name}\" is not an ability!");
                    continue;
                }

                (component as IActorAbility).Execute();
            }
        }

        public virtual void PostConvert()
        {
            WorldEntityManager.AddComponentData(ActorEntity, new ActorData { ActorId = ActorId, StateId = ActorStateId });

            if (Spawner == null) return;

            if (WorldEntityManager.HasComponent(Spawner.ActorEntity, typeof(NetworkSyncSend)))
            {
                WorldEntityManager.AddComponentData(ActorEntity, new NetworkSyncSend());
            }
        }

        public void Setup()
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                Debug.LogError(
                    "[ACTOR CONVERT TO ENTITY] Convert Entity failed because there was no Active World");
                return;
            }

            if (transform.parent != null && transform.parent.GetComponentInParent<ConvertToEntity>() != null)
            {
                return;
            }

            gameObject.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
        }

        private bool MustBeAbility(List<MonoBehaviour> actions)
        {
            foreach (var action in actions)
            {
                if (action is IActorAbility || action is null)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            try
            {
                if (ActorEntity.Index <= WorldEntityManager.EntityCapacity && WorldEntityManager.Exists(ActorEntity))
                {
                    WorldEntityManager.DestroyEntity(ActorEntity);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}