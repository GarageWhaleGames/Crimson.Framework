using System;
using Assets.Farm.Scripts.Core.Resources;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Farm.Scripts.Core.Buildings
{
    [Serializable]
    public struct BuildingUpgrade
    {
        public Resource[] CostResources;
        [HideInInspector] public DateTime StartTime;

        [InfoBox("Time in minutes")]
        public float TimeForUpgrade;
    }
}