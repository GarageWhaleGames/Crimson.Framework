using Assets.Farm.Scripts.Core.Resources.Interfaces;

namespace Assets.Farm.Scripts.Core.Buildings
{
    struct BuildingOrder
    {
        public int Time;
        public IResource[] IntakeResource;
        public IResource[] ProducedResource;
        public IResource[] Capacity;
    }
}
