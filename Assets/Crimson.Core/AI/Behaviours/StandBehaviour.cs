using Assets.Crimson.Core.AI.GeneralParams;
using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFramework.Example.AI
{
	[Serializable, HideMonoScript]
	public class StandBehaviour : MonoBehaviour, IActorAbility, IAIBehaviour
	{
		public BasePriority Priority = new BasePriority()
		{
			Value = 1
		};

		public IActor Actor { get; set; }

		public float Evaluate(Entity entity, AbilityAIInput ai, List<Transform> targets)
		{
			return Random.value * Priority.Value;
		}

		public bool SetUp(Entity entity, EntityManager dstManager)
		{
			return true;
		}

		public bool Behave(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			return true;
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
		}

		public void Execute()
		{
		}
	}
}