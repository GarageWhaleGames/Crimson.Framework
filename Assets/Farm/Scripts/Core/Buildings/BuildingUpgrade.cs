using Assets.Farm.Scripts.Core.Resources.Interfaces;

namespace Assets.Farm.Scripts.Core.Buildings
{
    struct BuildingUpgrade
    {
        private int Level;
        private IResource[] CostResources;
        private int Time;
        private BuildingOrder DefaultOrder;
    }
}
