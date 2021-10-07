using Assets.Farm.Scripts.Core.Resources;
using Crimson.Core.Common;
using Unity.Entities;

namespace Assets.Farm.Scripts.Common
{
    public class PlayerState : Actor
    {
        public readonly ResearchManager ResearchManager = new ResearchManager();
        public readonly ResourceStorage ResourceStorage = new ResourceStorage();

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            Setup();
        }
    }
}