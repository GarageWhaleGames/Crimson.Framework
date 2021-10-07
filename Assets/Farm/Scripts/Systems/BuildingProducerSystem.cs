using Assets.Farm.Scripts.Common;
using Assets.Farm.Scripts.Components.Buildings;
using Assets.Farm.Scripts.Components.Buildings.ComponentsData;
using System;
using Unity.Entities;

namespace Assets.Farm.Scripts.Systems
{
    public class BuildingProducerSystem : ComponentSystem
    {
        private EntityQuery _completionOrders;
        private EntityQuery _executingOrders;
        private EntityQuery _playerStates;
        private EntityQuery _startedOrders;

        protected override void OnCreate()
        {
            _playerStates = GetEntityQuery(
                ComponentType.ReadOnly<PlayerState>()
                ); ;
            _startedOrders = GetEntityQuery(
                ComponentType.ReadWrite<StartOrderData>(),
                ComponentType.ReadOnly<AbilityBuildingProducer>()
                );
            _executingOrders = GetEntityQuery(
                ComponentType.ReadWrite<ExectutingOrderData>(),
                ComponentType.ReadOnly<AbilityBuildingProducer>()
                );
            _completionOrders = GetEntityQuery(
                ComponentType.ReadWrite<CollectOrderData>(),
                ComponentType.ReadOnly<AbilityBuildingProducer>()
                );
        }

        protected override void OnUpdate()
        {
            Entities.With(_playerStates).ForEach((Entity entity, PlayerState state) =>
            {
                Entities.With(_startedOrders)
                .ForEach((Entity entity, AbilityBuildingProducer ability) => ProcessIdledOrders(entity, state, ability));
                Entities.With(_executingOrders)
                .ForEach((Entity entity, AbilityBuildingProducer ability) => ProcessStartedOrders(entity, ability));
                Entities.With(_completionOrders)
                .ForEach((Entity entity, AbilityBuildingProducer ability) => ProcessCompletedOrders(entity, state, ability));
            });
        }

        private void ProcessCompletedOrders(Entity entity, PlayerState state, AbilityBuildingProducer ability)
        {
            state.ResourceStorage.AddRange(ability.OrderData.ProducedResource);
            World.EntityManager.RemoveComponent<CollectOrderData>(entity);
            ability.State = ProducerState.Idle;
        }

        private void ProcessIdledOrders(Entity entity, PlayerState state, AbilityBuildingProducer ability)
        {
            World.EntityManager.RemoveComponent<StartOrderData>(entity);
            state.ResourceStorage.RemoveRange(ability.OrderData.IntakeResource);
            ability.OrderData.StartTime = DateTime.Now;
            ability.State = ProducerState.Started;
        }

        private void ProcessStartedOrders(Entity entity, AbilityBuildingProducer ability)
        {
            var durationTime = DateTime.Now - ability.OrderData.StartTime;
            if (durationTime.TotalMinutes > ability.OrderData.TimeForExecute)
            {
                World.EntityManager.RemoveComponent<ExectutingOrderData>(entity);
                ability.State = ProducerState.Completed;
            }
        }
    }
}