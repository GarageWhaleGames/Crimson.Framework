using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components.Forces;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Systems.Forces
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	internal class AdditionalForceSystem : ComponentSystem
	{
		private EntityQuery _explosionForce;
		private EntityQuery _forceQuery;
		private EntityQuery _inwardSourceQuery;

		protected override void OnCreate()
		{
			_inwardSourceQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityInwardForce>(),
				ComponentType.ReadOnly<InwardForceSourceTag>());

			_forceQuery = GetEntityQuery(
				ComponentType.ReadOnly<ForceData>(),
				ComponentType.ReadOnly<Rigidbody>());

			_explosionForce = GetEntityQuery(
				ComponentType.ReadOnly<ExplosionForceData>(),
				ComponentType.ReadOnly<Rigidbody>());
		}

		protected override void OnUpdate()
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			Entities.With(_inwardSourceQuery).ForEach(
				(AbilityInwardForce ability) =>
				{
					ability.AddForceInRadius();
				});

			Entities.With(_forceQuery).ForEach(
				(Entity entity, Rigidbody rigidBody, ref ForceData data) =>
				{
					if (rigidBody == null)
					{
						Debug.LogWarning($"[{nameof(AdditionalForceSystem)}] Not rigidbody on actor. Abort..");
						return;
					}
					rigidBody.AddForce(data.Direction * data.Force, (ForceMode)data.ForceMode);
					dstManager.RemoveComponent<ForceData>(entity);
				}
			);

			Entities.With(_explosionForce).ForEach(
				(Entity entity, Rigidbody rigidbody, ref ExplosionForceData data) =>
				{
					rigidbody.AddExplosionForce(data.Force, data.Position, data.Radius, data.UpwardModifier, (ForceMode)data.ForceMode);
					EntityManager.RemoveComponent<ExplosionForceData>(entity);
				});
		}
	}
}