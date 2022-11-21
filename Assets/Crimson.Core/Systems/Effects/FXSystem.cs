using Assets.Crimson.Core.Common.ComponentDatas;
using Assets.Crimson.Core.Components.FX.ForCamera;
using Assets.Crimson.Core.Components.Tags;
using Assets.Crimson.Core.Components.Tags.Effects;
using Crimson.Core.Common;
using Crimson.Core.Components;
using System.Linq;
using Unity.Entities;

namespace Assets.Crimson.Core.Systems.Effects
{
	public class FXSystem : ComponentSystem
	{
		private EntityQuery _damageEffectQuery;
		private EntityQuery _fadeInQuery;
		private EntityQuery _overdamageEffectQuery;
		private EntityQuery _shakeEffectQuery;

		protected override void OnCreate()
		{
			_fadeInQuery = GetEntityQuery(
				ComponentType.ReadOnly<FadeInFXTag>());
			_damageEffectQuery = GetEntityQuery(
				ComponentType.ReadOnly<DamageFXTag>());
			_shakeEffectQuery = GetEntityQuery(
				ComponentType.ReadOnly<ShakeFXTag>());
			_overdamageEffectQuery = GetEntityQuery(
				ComponentType.ReadOnly<OverdamageFXTag>(),
				ComponentType.ReadOnly<AbilityActorPlayer>(),
				ComponentType.ReadOnly<DeadActorTag>(),
				ComponentType.ReadOnly<DestructionPendingTag>(),
				ComponentType.ReadOnly<OverdamageData>());
		}

		protected override void OnUpdate()
		{
			Entities.WithAll<FadeInFXTag>().ForEach(
				(Entity entity, ShowFadeInFx showAbility) =>
				{
					var effectName = showAbility.EffectName;
					var duration = showAbility.Duration;
					Entities.WithAll<AbilityCameraFadeFX>().ForEach(
					(AbilityCameraFadeFX ability) =>
					{
						if (ability.ComponentName.Contains(effectName))
						{
							ability.Duration = duration;
							ability.Execute();
						}
					});
					EntityManager.RemoveComponent<FadeInFXTag>(entity);
				});

			Entities.With(_shakeEffectQuery).ForEach(
				(Entity entity) =>
				{
					EntityManager.RemoveComponent<ShakeFXTag>(entity);
				});

			Entities.With(_overdamageEffectQuery).ForEach(
				(Entity entity, AbilityActorPlayer actorPlayer, ref OverdamageData overdamageData) =>
				{
					EntityManager.RemoveComponent<OverdamageFXTag>(entity);
					if (actorPlayer.deadActorBehaviour == null)
					{
						return;
					}

					foreach (var name in actorPlayer.deadActorBehaviour.OnOverdamageActionsComponentNames)
					{
						var ability = actorPlayer.Actor.Abilities.FirstOrDefault(a =>
							a is IHasComponentName componentName && componentName.ComponentName.Equals(name));

						ability?.Execute();
					}
				});
		}
	}
}