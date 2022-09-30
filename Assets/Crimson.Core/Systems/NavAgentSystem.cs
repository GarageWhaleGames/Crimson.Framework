using Assets.Crimson.Core.Components;
using Assets.Crimson.Core.Utils;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Systems
{
	public class NavAgentSystem : ComponentSystem
	{
		private EntityQuery _syncMoveQuery;

		protected override void OnCreate()
		{
			_syncMoveQuery = GetEntityQuery(
				ComponentType.ReadWrite<PlayerInputData>(),
				ComponentType.ReadOnly<NavAgentAdapter>()
				);
		}
		protected override void OnUpdate()
		{
			Entities.With(_syncMoveQuery).ForEach(
				(NavMeshAgent agent, ref PlayerInputData data) =>
				{
					data.Move = agent.GetVelocity();
				});
		}
	}
}
