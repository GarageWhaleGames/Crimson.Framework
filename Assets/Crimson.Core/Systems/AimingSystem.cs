using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components.Perks;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Crimson.Core.Utils.LowLevel;
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class AimingSystem : ComponentSystem
	{
		private EntityQuery _aimingQuery;
		private EntityQuery _evaluateActionQuery;
		private EntityQuery _perkAimEvadeQuery;
		private EntityQuery _projectileToDestroyQuery;

		protected override void OnCreate()
		{
			_perkAimEvadeQuery = GetEntityQuery(
				ComponentType.Exclude<DeadActorTag>(),
				ComponentType.ReadOnly<AimData>());

			_aimingQuery = GetEntityQuery(
				ComponentType.ReadWrite<PlayerInputData>(),
				ComponentType.ReadOnly<AbilityPlayerInput>(),
				ComponentType.Exclude<DeadActorTag>(),
				ComponentType.Exclude<DestructionPendingTag>());

			_evaluateActionQuery = GetEntityQuery(
				ComponentType.ReadOnly<PlayerInputData>(),
				ComponentType.ReadOnly<AbilityPlayerInput>(),
				ComponentType.Exclude<DeadActorTag>(),
				ComponentType.Exclude<DestructionPendingTag>());

			_projectileToDestroyQuery = GetEntityQuery(
				ComponentType.ReadOnly<DestroyProjectileInPointData>(),
				ComponentType.ReadOnly<Transform>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_aimingQuery).ForEach(
				(Entity entity, AbilityPlayerInput mapping, ref PlayerInputData input) =>
				{
					if (mapping.inputSource != InputSource.UserInput) return;

					for (var i = 0; i <= input.CustomSticksInput.Length; i++)
					{
						var currentWeapons = mapping.customBindings
							.Where(b => b.index == i)
							.SelectMany(b => b.actions)
							.Where(a => a is IAimable aimable && aimable.AimingAvailable)
							.Cast<IAimable>();

						if (currentWeapons.Count() == 0) continue;

						var inputValue = (Vector2)input.CustomSticksInput[i];
						if (inputValue == Vector2.zero)
						{
							foreach (var item in currentWeapons)
							{
								item.ResetAiming();
							}
							continue;
						}

						foreach (var item in currentWeapons)
						{
							item.EvaluateAim(inputValue);
						}
					}
				});

			Entities.With(_evaluateActionQuery).ForEach(
				(Entity entity, AbilityPlayerInput mapping, ref PlayerInputData input) =>
				{
					foreach (var b in mapping.bindingsDict)
					{
						var aimables = b.Value.OfType<IAimable>();

						if (Math.Abs(input.CustomInput[b.Key]) < Constants.INPUT_THRESH)
						{
							foreach (var item in aimables)
							{
								item.ActionExecutionAllowed = true;
							}
							continue;
						}

						foreach (var item in aimables)
						{
							if (!item.ActionExecutionAllowed) return;

							(item as IActorAbility)?.Execute();

							if (item.AimingProperties.evaluateActionOptions == EvaluateActionOptions.EvaluateOnce) item.ActionExecutionAllowed = false;

							if (mapping.inputSource != InputSource.UserInput) return;

							PostUpdateCommands.AddComponent(entity, new NotifyButtonActionExecutedData
							{
								ButtonIndex = b.Key
							});
						}
					}
				});

			Entities.With(_projectileToDestroyQuery).ForEach(
				(Entity entity, Transform transform, ref DestroyProjectileInPointData point) =>
				{
					if (Math.Abs(transform.position.x - point.Point.x) < 1 &&
						Math.Abs(transform.position.z - point.Point.y) < 1)
					{
						transform.gameObject.DestroyWithEntity(entity);
					}
				});

			Entities.With(_perkAimEvadeQuery).ForEach(
				(ref AimData aimData) =>
				{
					if (aimData.Target == Entity.Null
						|| !EntityManager.HasComponent<PerkAimEvade>(aimData.Target))
					{
						return;
					}

					var perk = EntityManager.GetComponentObject<PerkAimEvade>(aimData.Target);
					perk.Execute();
				});
		}
	}
}