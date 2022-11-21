using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Crimson.Core.Components
{
	public class AbilityVisible : MonoBehaviour, IActorAbility
	{
		public UnityEvent<bool> OnChangeState;
		private Entity _entity;
		private EntityManager _entityManager;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Execute()
		{
			OnChangeState?.Invoke(!_entityManager.HasComponent<InvisibleTag>(_entity));
		}

		public void SetState(bool state)
		{
			OnChangeState?.Invoke(state);
		}
	}
}