using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityEnemyMark : MonoBehaviour, IActorAbility
	{
		public Vector3 Offset;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, new EnemyData(Offset));
		}

		public void Execute()
		{
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position + Offset, 1);
		}
	}
}