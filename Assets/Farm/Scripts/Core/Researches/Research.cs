using Assets.Farm.Scripts.Core.Resources;
using System;

namespace Assets.Farm.Scripts.Core.Researches
{
    [Serializable]
    public struct Research
    {
        public Resource[] Costs { get; set; }
        public string Description { get; set; }
        public int ID { get; set; }

        public string Name { get; set; }
        public int Time { get; set; }
    }
}