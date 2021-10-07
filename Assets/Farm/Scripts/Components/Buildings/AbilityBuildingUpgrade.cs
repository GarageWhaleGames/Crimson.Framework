using Assets.Farm.Scripts.Core.Buildings;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Assets.Farm.Scripts.Components.Buildings
{
    [HideMonoScript, NetworkSimObject]
    public class AbilityBuildingUpgrade : MonoBehaviour,
        IActorAbility,
        ILevelable
    {
        [NetworkSimData, ReadOnly]
        [CastToUI(nameof(CurrentLevel))]
        [LevelableValue]
        public int CurrentLevel = 1;

        [Space]
        [TitleGroup("Levelable properties")]
        [OnValueChanged(nameof(SetLevelableProperty))]
        public List<LevelableProperties> levelableProperties = new List<LevelableProperties>();

        [LevelableValue]
        public BuildingUpgrade UpgradeData;

        private List<FieldInfo> _levelablePropertiesInfoCached = new List<FieldInfo>();
        public IActor Actor { get; set; }
        public int Level { get => CurrentLevel; set => CurrentLevel = value; }

        public List<FieldInfo> LevelablePropertiesInfoCached
        {
            get
            {
                return _levelablePropertiesInfoCached.Count != 0
                    ? _levelablePropertiesInfoCached
                    : (_levelablePropertiesInfoCached = this.GetFieldsWithAttributeInfo<LevelableValue>());
            }
        }

        public List<LevelableProperties> LevelablePropertiesList
        {
            get => levelableProperties;
            set => levelableProperties = value;
        }

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;
            Actor.Owner = Actor;
        }

        public void Execute()
        {
        }

        public void SetLevel(int level)
        {
            this.SetAbilityLevel(level, LevelablePropertiesInfoCached, Actor);

            var abilities = Actor.Abilities.Where(a => a is ILevelable && !ReferenceEquals(a, this));
            foreach (ILevelable ability in abilities)
            {
                ability.SetLevel(Level);
            }
        }

        public void SetLevelableProperty()
        {
            this.SetLevelableProperty(LevelablePropertiesInfoCached);
        }

        private bool MustBeAbility(MonoBehaviour a)
        {
            return a is IActorAbility || a is null;
        }
    }
}