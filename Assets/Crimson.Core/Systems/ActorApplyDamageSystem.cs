using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorApplyDamageSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			Entities.WithAll<DamageData>().ForEach((Entity damageEntity, ref DamageData damageData) =>
			{
				if (!(dstManager.Exists(damageData.TargetEntity) &&
					  dstManager.Exists(damageData.AbilityOwnerEntity) &&
					  dstManager.HasComponent(damageData.AbilityOwnerEntity, typeof(AbilityActorPlayer)))) return;

				var abilityOwner = dstManager.GetComponentObject<AbilityActorPlayer>(damageData.AbilityOwnerEntity);

				if (dstManager.HasComponent<AbilityActorPlayer>(damageData.TargetEntity) &&
					!dstManager.HasComponent<DeadActorTag>(damageData.TargetEntity) &&
					!dstManager.HasComponent<DestructionPendingTag>(damageData.TargetEntity))
				{
					var target = dstManager.GetComponentObject<AbilityActorPlayer>(damageData.TargetEntity);

					if (target.IsAlive)
					{
						target.UpdateHealth((int)-damageData.DamageValue);
						abilityOwner.UpdateTotalDamageData((int)damageData.DamageValue);
						dstManager.AddComponent<DamagedActorTag>(damageData.TargetEntity);
					}

					if (!target.IsAlive)
					{
						abilityOwner.UpdateExperience(GameMeta.PointsForKill);
					}
				}

				if (dstManager.HasComponent<AbilityDestructibleObject>(damageData.TargetEntity))
				{
					var target = dstManager.GetComponentObject<AbilityDestructibleObject>(damageData.TargetEntity);

					if (target != null)
					{
						target.UpdateStrengthValue((int)-damageData.DamageValue);
						abilityOwner.UpdateTotalDamageData((int)damageData.DamageValue);
					}
				}

				PostUpdateCommands.DestroyEntity(damageEntity);
			});
		}
	}
}