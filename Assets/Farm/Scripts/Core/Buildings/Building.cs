using Assets.Farm.Scripts.Core.Buildings;
using Assets.Farm.Scripts.Core.Buildings.Interfaces;
using System.Collections.Generic;

namespace Assets.Farm.Scripts.Core
{
    internal class Building : IBuilding
    {
        private readonly Queue<BuildingOrder> _orders = new Queue<BuildingOrder>();
        private BuildingUpgrade[] _upgrades;
        public int ID { get; set; }
        public int Level => _upgrades != null ? _upgrades.Length : 0;
        public string Name { get; set; }

        public BuildingOrder[] Orders { get; set; }

        public void AddOrder(BuildingOrder order)
        {
            _orders.Enqueue(order);
        }

        public void DoUpgrade(BuildingUpgrade upgrade)
        {
        }
    }
}