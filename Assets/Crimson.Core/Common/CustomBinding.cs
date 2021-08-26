using System;
using System.Collections.Generic;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Crimson.Core.Common
{
    [Serializable]
    public struct CustomBinding
    {
        public int index;

        [ValidateInput("MustBeAbility", "Ability MonoBehaviours must derive from IActorAbility!")]
        public List<MonoBehaviour> actions;
        private bool MustBeAbility(List<MonoBehaviour> a)
        {
            return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
        }
    }
}