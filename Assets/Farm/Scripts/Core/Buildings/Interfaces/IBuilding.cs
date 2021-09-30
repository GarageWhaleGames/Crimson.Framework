namespace Assets.Farm.Scripts.Core.Buildings.Interfaces
{
    internal interface IBuilding
    {
        public int ID { get; set; }

        public int Level { get; }

        public string Name { get; set; }

        public BuildingOrder[] Orders { get; }
    }
}