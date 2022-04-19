using Crimson.Core.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.AI
{
	public interface IAIBehaviour
	{
		float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets);

		bool SetUp(Entity entity, EntityManager dstManager);

		bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData);
	}
}