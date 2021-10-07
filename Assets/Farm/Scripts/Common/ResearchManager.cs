using Assets.Farm.Scripts.Core.Researches;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Farm.Scripts.Common
{
    [Serializable]
    public class ResearchManager
    {
        [SerializeField, ReadOnly] private List<Research> _researches = new List<Research>();
    }
}