using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Farm.Scripts.Core.Resources
{
    public class ResourceStorage
    {
        private Dictionary<ResourceType, Resource> _items = new Dictionary<ResourceType, Resource>();

        public ResourceStorage()
        {
            InitDictByDefaultValues();
        }

        public float this[ResourceType type] => _items[type].Value;

        public void Add(Resource resource)
        {
            _items[resource.Type] += resource.Value;
        }

        public void AddRange(params Resource[] resources)
        {
            for (var i = 0; i < resources.Length; i++)
            {
                Add(resources[i]);
            }
        }

        public void Remove(Resource resource)
        {
            _items[resource.Type] -= resource.Value;
        }

        public void RemoveRange(params Resource[] resources)
        {
            for (var i = 0; i < resources.Length; i++)
            {
                Remove(resources[i]);
            }
        }

        private void InitDictByDefaultValues()
        {
            var types = Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>();
            foreach (var item in types)
            {
                _items.Add(item, new Resource());
            }
        }
    }
}