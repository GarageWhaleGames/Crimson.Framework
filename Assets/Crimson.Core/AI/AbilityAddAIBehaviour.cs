using Crimson.Core.AI;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.AI
{
	[HideMonoScript]
	public class AbilityAddAIBehaviour : TimerBaseBehaviour, IActorAbility
	{
		[ValidateInput(nameof(MustBeAI), "MonoBehaviours must derive from IAIBehaviour!")]
		public MonoBehaviour[] Behaviours;

		private Entity _entity;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			Timer.TimedActions.AddAction(Execute, 1);
			Timer.Start();
		}

		public void Execute()
		{
			if (Behaviours == null || Behaviours.Length == 0)
			{
				return;
			}

			var ability = Actor.GameObject.GetComponent<AbilityAIInput>();
			if (ability == null)
			{
				return;
			}

			AddTo(ability);
			Destroy(this);
		}

		private void AddTo(AbilityAIInput ability)
		{
			for (var i = 0; i < Behaviours.Length; i++)
			{
				ability.Behaviours.Add((IAIBehaviour)Behaviours[i]);
			}
		}

		private bool MustBeAI(MonoBehaviour[] behaviours)
		{
			return behaviours == null || behaviours.All(s => s is IAIBehaviour);
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			Behaviours = GetComponents<IAIBehaviour>().Cast<MonoBehaviour>().ToArray();
		}

#endif
	}
}