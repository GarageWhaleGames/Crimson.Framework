using Assets.Farm.Scripts.Common;
using Assets.Farm.Scripts.Components.Buildings.ComponentsData;
using Assets.Farm.Scripts.Core.Buildings;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Farm.Scripts.Components.Buildings
{
    public class AbilityBuildingProducer : MonoBehaviour, IActorAbility
    {
        //TODO:
        //  Может быть стоит перенести Order в абилити и его обрабатывать тем самым избежав состояний для Producer
        //  Добавить обработку Capacity
        [ReadOnly] public BuildingOrder OrderData;

        private EntityManager _dstManager;
        private Entity _entity;
        public IActor Actor { get; set; }

        public ProducerState State { get; set; }

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            _entity = entity;
            _dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Actor = actor;
            Actor.Owner = Actor;
        }

        public void Execute()
        {
            switch (State)
            {
                case ProducerState.Idle:
                    StartOrder();
                    break;

                case ProducerState.Started:
                    break;

                case ProducerState.Completed:
                    CollectResources();
                    break;

                default:
                    break;
            }
        }

        private void CollectResources()
        {
            _dstManager.AddComponent<CollectOrderData>(_entity);
        }

        private void StartOrder()
        {
            _dstManager.AddComponent<StartOrderData>(_entity);
        }
    }
}