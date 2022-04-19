using Crimson.Core.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	public class AISetupSystem : ComponentSystem
	{
		private EntityQuery _queryAI;
		private EntityQuery _queryTargets;

		private List<Transform> _targets;

		protected override void OnCreate()
		{
			_queryAI = GetEntityQuery(
				ComponentType.ReadWrite<SetupAITag>(),
				ComponentType.ReadOnly<AIInputData>(),
				ComponentType.ReadOnly<AbilityAIInput>(),
				ComponentType.ReadOnly<Transform>(),
				ComponentType.Exclude<DeadActorTag>(),
				ComponentType.Exclude<DestructionPendingTag>(),
				ComponentType.Exclude<EvaluateAITag>(),
				ComponentType.Exclude<NetworkInputData>());
		}

		protected override void OnUpdate()
		{
			_targets = new List<Transform>();

			Entities.With(_queryAI).ForEach(
				(Entity entity, Transform transform, AbilityAIInput ai) =>
				{
					if (ai.activeBehaviour == null)
					{
						return;
					}

					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (ai.activeBehaviourPriority == 0)
					{
						return;
					}

					if (!ai.activeBehaviour.SetUp(entity, World.EntityManager))
					{
						ai.EvaluateAll();
					}

					World.EntityManager.RemoveComponent<SetupAITag>(entity);
				}
			);
		}
	}
}