using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityAnimationEvent : MonoBehaviour, IActorAbility
	{
		public IActor Actor { get; set; }

		private Entity _entity;
		private EntityManager _dstManager;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		}

		public void Execute()
		{
		}

		public void RaiseMeleeAttackEvent()
		{
			_dstManager.AddComponentData(_entity, new AnimationMeleeAttackTag());
		}
	}
}