using Assets.Crimson.Core.Common.Buffs;
using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Perks
{
	public class PerkActorRessurect : MonoBehaviour, IActorAbility, IPerkAbility
	{
		[Range(0, 100)]
		public float EnergyPercent = 100;

		[Range(0, 100)]
		public float HealthPercent = 100;

		private Entity _entity;
		private EntityManager _entityManager;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			Actor = actor;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Apply(IActor target)
		{
			_entityManager.AddComponentData(_entity, new RessurectAnimationTag());
			_entityManager.AddComponentData(_entity, new RessurectTag());

			_entityManager.GetBuffer<HealthPercentPerTimeBuffData>(_entity).Add(new HealthPercentPerTimeBuffData(HealthPercent));
			_entityManager.GetBuffer<EnergyPercentPerTimeBuffData>(_entity).Add(new EnergyPercentPerTimeBuffData(EnergyPercent));
		}

		[Button]
		public void Execute()
		{
			Apply(Actor);
		}

		public void Remove()
		{
			if (Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Remove(this);
			}

			Destroy(this);
		}
	}
}