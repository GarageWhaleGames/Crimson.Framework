using System;
using Assets.Farm.Scripts.Core.Resources;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Farm.Scripts.Core.Buildings
{
    [Serializable]
    public struct BuildingOrder
    {
        public Resource[] Capacity;
        public Resource[] IntakeResource;
        public Resource[] ProducedResource;

        [HideInInspector]
        public DateTime StartTime;

        [InfoBox("Time in minutes")]
        public float TimeForExecute;
    }
}