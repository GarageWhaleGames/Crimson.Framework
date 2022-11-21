using Assets.Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Crimson.Core.Systems
{
	public class VisibleSystem : ComponentSystem
	{
		private EntityQuery _abilityVisibleQuery;
		private EntityQuery _uiVisibleQuere;

		protected override void OnCreate()
		{
			_uiVisibleQuere = GetEntityQuery(
				ComponentType.ReadOnly<AbilityUIActorVisible>());
			_abilityVisibleQuery = GetEntityQuery(ComponentType.ReadOnly<AbilityVisible>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_uiVisibleQuere).ForEach((AbilityUIActorVisible ability) => { ability.Execute(); });
			Entities.With(_abilityVisibleQuery).ForEach((AbilityVisible ability) => { ability.Execute(); });
		}
	}
}