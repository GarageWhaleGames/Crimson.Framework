using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Magnets
{
	public class AbilityMagnetQuestPlace : MonoBehaviour, IActorAbilityTarget
	{
		public ComponentNameFilter ComponentNameFilter;
		public TagFilter TagFilter;

		public ThresholdFilter ThresholdFilter;

		public ActionsList ActionsList;
		public IActor AbilityOwnerActor { get; set; }

		private EntityManager _entityManager;

		public IActor Actor { get; set; }
		public IActor TargetActor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			AbilityOwnerActor = actor;

			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			ActionsList.Init();
			ThresholdFilter.SetTarget(transform);
		}

		public void Execute()
		{
			if (TargetActor == null)
			{
				Debug.LogWarning($"<b>[{nameof(AbilityMagnetQuestPlace).ToUpper()}]</b> Cannot execute because target actor is null");
				return;
			}

			if (_entityManager.HasComponent<MagnetRigidbodyData>(TargetActor.ActorEntity))
			{
				return;
			}

			var componentNameResult = ComponentNameFilter.Filter(TargetActor);
			var tagResult = TagFilter.Filter(TargetActor);
			var thresholdResult = ThresholdFilter.Filter(TargetActor);

			if (!ComponentNameFilter.Filter(TargetActor)
				|| !TagFilter.Filter(TargetActor)
				|| !ThresholdFilter.Filter(TargetActor))
			{
				return;
			}

			ActionsList.SetTarget(TargetActor);
			ActionsList.Execute();
		}
	}
}