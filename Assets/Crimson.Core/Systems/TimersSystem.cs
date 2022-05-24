using Crimson.Core.Common;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;

namespace Crimson.Core.Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class TimersSystem : ComponentSystem
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                ComponentType.ReadOnly<TimerData>(),
                ComponentType.ReadOnly<TimerComponent>());
        }

        protected override void OnUpdate()
        {
            var dt = UnityEngine.Time.deltaTime;

            Entities.With(_query).ForEach(
                (Entity entity, TimerComponent timer) =>
                {
                    for (var i = timer.TimedActions.Count - 1; i >= 0; i--)
                    {
                        var timerAction = timer.TimedActions[i];
                        timerAction.Delay -= dt;
                        timer.TimedActions[i] = timerAction;

                        if (!(timerAction.Delay <= 0f)) continue;
                        var a = timer.TimedActions[i].Act;
                        timer.TimedActions.RemoveAt(i);

                        a?.Invoke();
                    }
                });
        }
    }
}