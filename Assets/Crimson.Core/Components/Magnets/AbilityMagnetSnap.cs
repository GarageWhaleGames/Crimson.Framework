using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Magnets;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Magnets
{
	public class AbilityMagnetSnap : MonoBehaviour, IActorAbilityTarget
	{
		public float _distanceThreshold = .5f;

		[Range(0, 180)]
		public float _rotationTheshold = 5;

		public float _snapSpeed = 5;
		private Entity _entity;

		private EntityManager _entityManager;
		public IActor AbilityOwnerActor { get; set; }
		public IActor Actor { get; set; }
		public IActor TargetActor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			AbilityOwnerActor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		}

		public void Execute()
		{
			if (TargetActor == null)
			{
				return;
			}

			var hasMagnetAbility = _entityManager.HasComponent<AbilityMagnet>(TargetActor.ActorEntity);
			var isUsing = _entityManager.HasComponent<MagnetRigidbodyData>(TargetActor.ActorEntity);
			if (!hasMagnetAbility || isUsing)
			{
				return;
			}

			if (!_entityManager.HasComponent<MagnetSnapData>(TargetActor.ActorEntity))
			{
				var data = new MagnetSnapData
				{
					Target = _entity,
					DistanceThreshold = _distanceThreshold,
					RotationTheshold = _rotationTheshold,
					SnapSpeed = _snapSpeed
				};
				_entityManager.AddComponentData(TargetActor.ActorEntity, data);
			}
		}
	}
}