using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Utils.LowLevel.Obsolete
{
    public static class RunFixedUpdateSystems
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SetFixedUpdate()
        {
            //FixedRateUtils.EnableFixedRateSimple(World.DefaultGameObjectInjectionWorld.GetExistingSystem<FixedUpdateGroup>(), UnityEngine.Time.fixedDeltaTime);
        }
        
    }
}