using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Components.Perks
{
	[HideMonoScript]
	public class PerkPushNavAgent : MonoBehaviour, IActorAbility, IPerkAbility
	{
		public EntityManager _dstManager;
		public Entity _entity;

		public Vector3 Direction;

		public float Value = 25;
		private NavMeshAgent _agent;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
			_agent = GetComponent<NavMeshAgent>();
		}

		public void Apply(IActor target)
		{
			if (!target.AppliedPerks.Contains(this))
			{
				target.AppliedPerks.Add(this);
			}
			if (!_dstManager.Exists(target.ActorEntity))
			{
				return;
			}
			var direction = transform.TransformDirection(Direction).normalized;
			if (!_agent.isOnNavMesh)
			{
				_agent.enabled = false;
				_agent.enabled = true;
			}
			_agent.Move(direction * Value);
		}

		public void Execute()
		{
			Apply(Actor);
		}

		public void Remove()
		{
			if (Actor != null && Actor.AppliedPerks.Contains(this))
			{
				Actor.AppliedPerks.Remove(this);
			}

			Destroy(this);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawLine(Vector3.zero, Direction);
		}
	}
}