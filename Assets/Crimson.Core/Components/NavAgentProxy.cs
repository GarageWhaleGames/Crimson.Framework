using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Components
{
	[HideMonoScript]
	public class NavAgentProxy : MonoBehaviour, IActorAbility
	{
		private NavMeshAgent _agent;

		public IActor Actor { get; set; }

		public float Speed { get => _agent.speed; set => _agent.speed = value; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_agent = GetComponent<NavMeshAgent>();
			if (_agent != null)
			{
				_agent.isStopped = false;
				_agent.velocity = Vector3.zero;
			}
		}

		public void Execute()
		{
		}

		public void Move(Vector3 delta)
		{
			if (_agent != null)
			{
				_agent.velocity = delta;
			}
			else
			{
				var newPos = transform.position + delta;
				transform.position = newPos;
			}
		}
	}
}