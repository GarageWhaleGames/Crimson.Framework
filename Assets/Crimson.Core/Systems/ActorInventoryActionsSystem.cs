﻿using Assets.Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;

namespace Assets.Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class ActorInventoryActionsSystem : ComponentSystem
    {
        private EntityQuery _addActionsQuery;
        private EntityQuery _removeActionsQuery;

        private EntityManager Manager => World.EntityManager;

        protected override void OnCreate()
        {
            _addActionsQuery = GetEntityQuery(
                ComponentType.ReadOnly<AbilityInventory>(),
                ComponentType.ReadOnly<AddItemData>()
            );

            _removeActionsQuery = GetEntityQuery(
                ComponentType.ReadOnly<AbilityInventory>(),
                ComponentType.ReadOnly<RemoveItemData>()
            );
        }

        protected override void OnUpdate()
        {
            Entities.With(_addActionsQuery).ForEach(
                (Entity entity, AbilityInventory inventory, ref AddItemData itemData) =>
                {
                    Manager.RemoveComponent<AddItemData>(entity);
                    inventory.Add(itemData.Item);
                }
            );

            Entities.With(_removeActionsQuery).ForEach(
                (Entity entity, AbilityInventory inventory, ref RemoveItemData itemData) =>
                {
                    Manager.RemoveComponent<RemoveItemData>(entity);
                    inventory.Remove(itemData.Item);
                }
            );
        }
    }
}