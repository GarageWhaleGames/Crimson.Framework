using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crimson.Core.Components
{
	[HideMonoScript]
	public class AbilityAIInput : TimerBaseBehaviour, IActorAbility, IAIModule
	{
		public IActor Actor { get; set; }

		public List<IAIBehaviour> Behaviours = new List<IAIBehaviour>();

		[MinMaxSlider(0, 30, true)] public Vector2 behaviourUpdatePeriod = new Vector2(2f, 6f);

		[ReadOnly, ShowInInspector] private string _currentBehaviour;

		[HideInInspector]
		public IAIBehaviour activeBehaviour
		{
			get => _activeBehaviour;
			set
			{
				_activeBehaviour = value;
				if (value != null)
				{
					_currentBehaviour = value.GetType().Name;
				}
			}
		}

		[HideInInspector] public float activeBehaviourPriority = 0;

		private Entity _entity;
		private EntityManager _dstManager;
		private IAIBehaviour _activeBehaviour;

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			Behaviours = new List<IAIBehaviour>();

			_dstManager.AddComponent<NetworkSyncReceive>(entity);

			StartTimer();
			EvaluateAll();
		}

		public void EvaluateAll()
		{
			this.RemoveAction(EvaluateAll);

			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (_dstManager.Exists(_entity))
			{
				_dstManager.RemoveComponent<SetupAITag>(_entity);
				_dstManager.AddComponent<EvaluateAITag>(_entity);
			}

			if (TimerActive)
			{
				this.AddAction(EvaluateAll, Random.Range(behaviourUpdatePeriod[0], behaviourUpdatePeriod[1]));
			}
		}

		public void Execute()
		{
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (activeBehaviour != null && activeBehaviour is Assets.Crimson.Core.AI.Interfaces.IDrawGizmos drawer)
			{
				drawer.DrawGizmos();
			}
		}

#endif
	}
}