using Assets.Crimson.Core.Common.Buffs;
using Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Crimson.Core.Systems
{
	public class ActorBuffSystem : ComponentSystem
	{
		private EntityQuery _energyPercentQuery;
		private EntityQuery _healthPercentQuery;

		protected override void OnCreate()
		{
			_healthPercentQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityActorPlayer>(),
				ComponentType.ReadWrite<HealthPercentPerTimeBuffData>());

			_energyPercentQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityActorPlayer>(),
				ComponentType.ReadWrite<EnergyPercentPerTimeBuffData>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_healthPercentQuery).ForEach(
				(Entity entity, AbilityActorPlayer abilityPlayer) =>
				{
					var buffer = EntityManager.GetBuffer<HealthPercentPerTimeBuffData>(entity);
					for (var i = 0; i < buffer.Length; i++)
					{
						var buff = buffer[i];
						buff.Duration -= UnityEngine.Time.deltaTime;
						var value = abilityPlayer.Stats.Health.MaxLimit * buff.Percent / 100;
						abilityPlayer.UpdateHealth(value);

						if (buff.Duration <= 0)
						{
							buffer.RemoveAt(i);
						}
					}
				});

			Entities.With(_energyPercentQuery).ForEach(
				(Entity entity, AbilityActorPlayer abilityPlayer) =>
				{
					var buffer = EntityManager.GetBuffer<EnergyPercentPerTimeBuffData>(entity);
					for (var i = 0; i < buffer.Length; i++)
					{
						var buff = buffer[i];
						buff.Duration -= UnityEngine.Time.deltaTime;
						var value = abilityPlayer.Stats.Energy.MaxLimit * buff.Percent / 100;
						abilityPlayer.UpdateEnergy(value);

						if (buff.Duration <= 0)
						{
							buffer.RemoveAt(i);
						}
					}
				});
		}
	}
}