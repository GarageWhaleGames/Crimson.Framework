using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Components
{
	public class AbilityFollowActor : MonoBehaviour, IActorAbility
	{
		public AbilityFindTargetActor AbilityTarget;
		public float PositionThreshold = 1;
		private Entity _entity;
		private EntityManager _entityManager;
		private NavMeshAgent _navMeshAgent;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_navMeshAgent = GetComponent<NavMeshAgent>();
		}

		public void Execute()
		{
			if (_navMeshAgent != null)
			{
				_navMeshAgent.enabled = false;
			}

			var followData = new ActorFollowData()
			{
				PositionThreshold = PositionThreshold
			};
			_entityManager.AddComponentData(_entity, followData);
		}
	}
}