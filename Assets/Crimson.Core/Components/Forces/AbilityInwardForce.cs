using Assets.Crimson.Core.Common.Filters;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Forces
{
	[HideMonoScript]
	public class AbilityInwardForce : MonoBehaviour, IActorAbility
	{
		public EntityManager _dstManager;
		public Entity _entity;

		public bool ExecuteOnAwake = true;
		public float Force = 25;

		public ForceMode ForceMode;
		public float Radius = 5;

		public TagFilter TagFilter;

		public IActor Actor { get; set; }

		private InwardForce _force => new InwardForce
		{
			Force = Force,
			SourcePosition = transform.position,
			ForceMode = (int)ForceMode
		};

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
			if (ExecuteOnAwake)
			{
				Execute();
			}
		}

		public void AddForceInRadius()
		{
			var actors = GetActorsInRadius();
			AddForceTo(actors);
		}

		public void Execute()
		{
			_dstManager.AddComponentData(_entity, new InwardForceSourceTag());
		}

		private void AddForceTo(IEnumerable<IActor> actors)
		{
			foreach (var actor in actors)
			{
				AddForceTo(actor);
			}
		}

		private void AddForceTo(IActor actor)
		{
			if (!_dstManager.Exists(actor.ActorEntity))
			{
				return;
			}
			_dstManager.AddComponent<AdditionalForceActorTag>(actor.ActorEntity);
			if (_dstManager.HasComponent<InwardForce>(actor.ActorEntity))
			{
				_dstManager.SetComponentData(actor.ActorEntity, _force);
			}
			else
			{
				_dstManager.AddComponentData(actor.ActorEntity, _force);
			}
		}

		private IEnumerable<IActor> GetActorsInRadius()
		{
			var results = Physics.OverlapSphere(transform.position, Radius);
			var actors = results.Select(s => s.GetComponent<IActor>()).Where(s => s != null);
			return actors.Where(s => TagFilter.Filter(s));
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, Radius);
		}

#endif
	}
}