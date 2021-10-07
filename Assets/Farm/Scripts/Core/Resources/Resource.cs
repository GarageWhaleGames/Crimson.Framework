using System;
using Unity.Mathematics;

namespace Assets.Farm.Scripts.Core.Resources
{
    [Serializable]
    public struct Resource
    {
        public ResourceType Type;
        public float Value;

        public static Resource operator -(Resource source, float value)
        {
            return new Resource()
            {
                Type = source.Type,
                Value = math.clamp(source.Value - value, 0, float.MaxValue)
            };
        }

        public static Resource operator --(Resource source)
        {
            return new Resource()
            {
                Type = source.Type,
                Value = math.clamp(source.Value--, 0, float.MaxValue)
            };
        }

        public static Resource operator *(Resource source, float value)
        {
            return new Resource()
            {
                Type = source.Type,
                Value = math.clamp(source.Value * value, 0, float.MaxValue)
            };
        }

        public static Resource operator /(Resource source, float value)
        {
            return new Resource()
            {
                Type = source.Type,
                Value = source.Value / value
            };
        }

        public static Resource operator +(Resource source, float value)
        {
            return new Resource()
            {
                Type = source.Type,
                Value = source.Value + value
            };
        }

        public static Resource operator ++(Resource source)
        {
            return new Resource()
            {
                Type = source.Type,
                Value = source.Value++
            };
        }
    }
}