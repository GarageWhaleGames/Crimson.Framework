using Assets.Crimson.Core.Components.FX.ForCamera;
using Assets.Crimson.Core.Components.Tags.Effects;
using Unity.Entities;

namespace Assets.Crimson.Core.Systems.Effects
{
	public class FXSystem : ComponentSystem
	{
		private EntityQuery _damageEffectQuery;
		private EntityQuery _shakeEffectQuery;

		protected override void OnCreate()
		{
			_damageEffectQuery = GetEntityQuery(
				ComponentType.ReadOnly<DamageFXTag>());
			_shakeEffectQuery = GetEntityQuery(
				ComponentType.ReadOnly<ShakeFXTag>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_damageEffectQuery).ForEach(
				(Entity entity) =>
				{
					Entities.WithAll<AbilityCameraShake>()
					.ForEach((AbilityCameraShake ability) =>
					{
						if (ability.Actor.Owner.ActorEntity == entity)
						{
							ability.Execute();
						}
					});

					Entities.WithAll<AbilityVignetteCameraFX>()
					.ForEach((AbilityVignetteCameraFX ability) =>
					{
						if (ability.Actor.Owner.ActorEntity == entity)
						{
							ability.Execute();
						}
					});

					EntityManager.RemoveComponent<DamageFXTag>(entity);
				});

			Entities.With(_shakeEffectQuery).ForEach(
				(Entity entity) =>
				{
					EntityManager.RemoveComponent<ShakeFXTag>(entity);
				});
		}
	}
}