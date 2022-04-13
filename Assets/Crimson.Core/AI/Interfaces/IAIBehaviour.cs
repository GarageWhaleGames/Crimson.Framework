using Crimson.Core.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.AI
{
	public interface IAIBehaviour
	{
		string XAxis { get; }
		string[] AdditionalModes { get; }
		bool NeedCurve { get; }
		bool NeedTarget { get; }
		bool NeedActions { get; }

		bool HasDistanceLimit { get; }

		float Evaluate(Entity entity, AIBehaviourSetting behaviourSetting, AbilityAIInput ai, List<Transform> targets);

		bool SetUp(Entity entity, EntityManager dstManager);

		bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData);
	}
}