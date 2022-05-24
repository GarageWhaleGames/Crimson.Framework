using Crimson.Core.Common;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class SphereAddForce : MonoBehaviour, IActorAbility
	{
		public EntityManager _dstManager;
		public Entity _entity;

		public float Power = 25;
		public float Radius = 5;
		public ForceMode Mode = ForceMode.Force;

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
		}

		public void Execute()
		{
			var targets = Physics.OverlapSphere(transform.position, Radius);
			AddForceTo(targets);
		}

		private void AddForceTo(Collider[] targets)
		{
			for (var i = 0; i < targets.Length; i++)
			{
				AddForceTo(targets[i]);
			}
		}

		private void AddForceTo(Collider targetCollider)
		{
			if (targetCollider == null || targetCollider.attachedRigidbody == null)
			{
				return;
			}

			var target = targetCollider.GetComponent<Rigidbody>();

			target.AddExplosionForce(Power, transform.position, Radius, 3.0f, Mode);
		}
#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, Radius);
		}

#endif
	}
}