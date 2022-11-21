using Crimson.Core.Common;
using Crimson.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Utils
{
	public static class AbilityUtils
	{
		public static List<Component> CopyBindablePerk(this IPerkAbilityBindable perk, IActor target,
			int bindingIndex = -1)
		{
			var componentsToCopy = perk.PerkRelatedComponents.Select(m => m as Component).ToList();
			componentsToCopy.Add(perk as Component);

			var newComponents = target.GameObject.CopyComponentsWithLinks(componentsToCopy.ToArray());

			if (bindingIndex != -1)
			{
				var addActionsToPlayerInput = newComponents.FirstOrDefault(c =>
					c is AbilityAddActionsToPlayerInput) as AbilityAddActionsToPlayerInput;

				if (addActionsToPlayerInput == null)
				{
					Debug.LogError("Bindable perk must contain AbilityAddActionsToPlayerInput component!");
					return null;
				}

				addActionsToPlayerInput.customBinding.index = bindingIndex;
			}

			var actorEntity = target.ActorEntity;

			foreach (var component in newComponents.OfType<IActorAbility>())
			{
				component.AddComponentData(ref actorEntity, target);
			}

			return newComponents;
		}

		public static void EvaluateAim(this IAimable aiming, IActor actor, Vector2 pos)
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (aiming.SpawnedAimingPrefab == null)
			{
				GameObject prefabToSpawn;
				switch (aiming.AimingProperties.aimingType)
				{
					case AimingType.AimingArea:
						prefabToSpawn = aiming.AimingProperties.aimingAreaPrefab;
						break;

					case AimingType.SightControl:
						prefabToSpawn = aiming.AimingProperties.sightPrefab;
						break;

					case AimingType.Circle:
						prefabToSpawn = aiming.AimingProperties.circlePrefab;
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				if (prefabToSpawn == null)
				{
					return;
				}

				var spawnAbility = actor.GameObject.AddComponent<AbilityActorSimpleSpawn>();

				spawnAbility.Actor = actor;

				spawnAbility.objectToSpawn = prefabToSpawn;
				spawnAbility.ownerType = OwnerType.CurrentActor;
				spawnAbility.spawnerType = SpawnerType.CurrentActor;
				spawnAbility.DestroyAfterSpawn = true;

				spawnAbility.Execute();

				if (spawnAbility.spawnedObject == null)
				{
					return;
				}

				aiming.SpawnedAimingPrefab = spawnAbility.spawnedObject;
			}

			if (!dstManager.HasComponent<ActorEvaluateAimingAnimData>(actor.ActorEntity))
			{
				dstManager.AddComponentData(actor.ActorEntity, new ActorEvaluateAimingAnimData { AimingActive = true });
				dstManager.AddComponentData(actor.ActorEntity, new AimingData { Direction = new Vector3(pos.x, 0, pos.y) });
			}
			else
			{
				var existingComponent = dstManager.GetComponentData<ActorEvaluateAimingAnimData>(actor.ActorEntity);
				existingComponent.AimingActive = true;

				var aimData = dstManager.GetComponentData<AimingData>(actor.ActorEntity);
				aimData.Direction = new Vector3(pos.x, 0, pos.y);

				dstManager.SetComponentData(actor.ActorEntity, existingComponent);
				dstManager.SetComponentData(actor.ActorEntity, aimData);
			}

			aiming.EvaluateAimBySelectedType(pos);
		}

		public static Vector3 EvaluateAimByArea(this IAimable aiming, Vector2 pos)
		{
			var currentRotation = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;

			var newRot = aiming.SpawnedAimingPrefab.transform.eulerAngles;
			newRot.y = currentRotation;
			aiming.SpawnedAimingPrefab.transform.eulerAngles = newRot;
			return aiming.SpawnedAimingPrefab.transform.forward;
		}

		public static Vector3 EvaluateAimBySight(this IAimable aiming, IActor actor, Vector2 pos)
		{
			if (!aiming.AimingProperties.reverseControls) pos *= -1;

			var newSightPos = actor.GameObject.transform.position - new Vector3(
								  pos.x * aiming.AimingProperties.sightDistance,
								  0, pos.y * aiming.AimingProperties.sightDistance);

			aiming.SpawnedAimingPrefab.transform.position = newSightPos;
			var rotation = Quaternion.LookRotation(actor.GameObject.transform.position - newSightPos);
			aiming.SpawnedAimingPrefab.transform.rotation = rotation;

			return aiming.SpawnedAimingPrefab.transform.position;
		}

		public static void Exectute(this IEnumerable<IActorAbility> abilities)
		{
			foreach (var ability in abilities)
			{
				ability.Execute();
			}
		}

		public static void Exectute(this IEnumerable<IActorAbilityTarget> abilities, IActor target)
		{
			foreach (var ability in abilities)
			{
				ability.TargetActor = target;
				ability.Execute();
			}
		}

		public static void FinishAbilityCooldownTimer(this ICooldownable timer, IActor actor)
		{
			var actorPlayer = actor.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;

			if (actorPlayer == null || !actorPlayer.actorToUI) return;

			if (!(timer is IBindable bindable)) return;

			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			var uiReceiverList = actorPlayer.UIReceiverList;

			var abilityPlayerInput = actor.Abilities.FirstOrDefault(a => a is AbilityPlayerInput) as AbilityPlayerInput;

			if (abilityPlayerInput == null) return;

			var currentBinding =
				abilityPlayerInput.customBindings.FirstOrDefault(b => b.index == bindable.BindingIndex);

			if (!currentBinding.Equals(new CustomBinding()) && currentBinding.actions.FirstOrDefault(a =>
					a is IAimable aimable && aimable.AimingAvailable && aimable.DeactivateAimingOnCooldown) != null)
			{
				uiReceiverList.ForEach(r => ((UIReceiver)r).SetCustomButtonOnCooldown(bindable.BindingIndex, false));
			}

			if (dstManager.HasComponent<BindedActionsCooldownData>(actor.ActorEntity))
			{
				var existingComponent = dstManager.GetComponentData<BindedActionsCooldownData>(actor.ActorEntity);

				if (!existingComponent.ReadyToUseBindingIndexes.Contains(bindable.BindingIndex))
				{
					existingComponent.ReadyToUseBindingIndexes.Add(bindable.BindingIndex);
					dstManager.SetComponentData(actor.ActorEntity, existingComponent);
				}
				return;
			}

			dstManager.AddComponentData(actor.ActorEntity,
				new BindedActionsCooldownData
				{ ReadyToUseBindingIndexes = new FixedList32Bytes<int> { bindable.BindingIndex } });
		}

		public static void ResetAiming(this IAimable aiming, IActor actor)
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (aiming.SpawnedAimingPrefab == null) return;

			if (!dstManager.HasComponent<ActorEvaluateAimingAnimData>(actor.ActorEntity))
			{
				dstManager.AddComponentData(actor.ActorEntity, new ActorEvaluateAimingAnimData { AimingActive = false });
			}
			else
			{
				var existingComponent = dstManager.GetComponentData<ActorEvaluateAimingAnimData>(actor.ActorEntity);
				existingComponent.AimingActive = false;

				dstManager.SetComponentData(actor.ActorEntity, existingComponent);
			}

			UnityEngine.Object.Destroy(aiming.SpawnedAimingPrefab);
		}

		public static void SetAbilityLevel(this ILevelable ability, int level,
													List<FieldInfo> levelablePropertiesInfo, IActor actor, IActor target = null)
		{
			ability.Level = level;

			foreach (var levelableProperty in ability.LevelablePropertiesList)
			{
				var fieldInfo =
					levelablePropertiesInfo.FirstOrDefault(f => f.Name == levelableProperty.propertyName);

				if (fieldInfo == null || fieldInfo.FieldType != levelableProperty.modifier.GetType()) continue;

				var fieldValue = fieldInfo.GetValue(ability);

				var newValue = 0.0f;

				switch (levelableProperty.levelablePropertyAction)
				{
					case ModifiablePropertiesActions.Multiply:
						newValue = target == null || target == actor.Owner
							? (float)fieldValue * levelableProperty.modifier
							: (float)((float)fieldValue * Math.Pow(levelableProperty.modifier, ability.Level - 1));
						break;

					case ModifiablePropertiesActions.Add:
						newValue = target == null || target == actor.Owner
							? (float)fieldValue + levelableProperty.modifier
							: (float)fieldValue + (ability.Level - 1) * levelableProperty.modifier;
						break;
				}

				fieldInfo.SetValue(ability, newValue);
			}
		}

		public static void SetLevelableProperty(this ILevelable levelable, List<FieldInfo> levelablePropertiesInfoCached)
		{
			if (!levelable.LevelablePropertiesList.Any() ||
				!levelable.LevelablePropertiesList.Last().levelablePropertiesInfo.IsNullOrEmpty()) return;

			var lastStruct = levelable.LevelablePropertiesList.Last();
			lastStruct.levelablePropertiesInfo = levelablePropertiesInfoCached.Select(f => f.Name).ToList();

			levelable.LevelablePropertiesList.RemoveAt(levelable.LevelablePropertiesList.Count - 1);
			levelable.LevelablePropertiesList.Add(lastStruct);
		}

		public static void StartAbilityCooldownTimer(this ICooldownable timer, IActor actor)
		{
			var actorPlayer = actor.Abilities.FirstOrDefault(a => a is AbilityActorPlayer) as AbilityActorPlayer;

			if (actorPlayer == null || !actorPlayer.actorToUI) return;

			if (!(timer is IBindable)) return;

			var bindable = (IBindable)timer;

			if (bindable.BindingIndex < 0) return;

			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			var uiReceiverList = actorPlayer.UIReceiverList;

			if (dstManager.HasComponent<BindedActionsCooldownData>(actor.ActorEntity))
			{
				var existingComponent = dstManager.GetComponentData<BindedActionsCooldownData>(actor.ActorEntity);

				if (existingComponent.OnCooldownBindingIndexes.Contains(bindable.BindingIndex)) return;

				existingComponent.OnCooldownBindingIndexes.Add(bindable.BindingIndex);
				dstManager.SetComponentData(actor.ActorEntity, existingComponent);
			}
			else
			{
				dstManager.AddComponentData(actor.ActorEntity, new BindedActionsCooldownData
				{
					OnCooldownBindingIndexes = new FixedList32Bytes<int> { bindable.BindingIndex },
				});
			}

			var abilityPlayerInput = actor.Abilities.FirstOrDefault(a => a is AbilityPlayerInput) as AbilityPlayerInput;

			if (abilityPlayerInput == null) return;

			var currentBinding = abilityPlayerInput.customBindings.FirstOrDefault(b => b.index == bindable.BindingIndex);

			if (!currentBinding.Equals(new CustomBinding()) && currentBinding.actions.FirstOrDefault(a =>
					a is IAimable aimable && aimable.AimingAvailable && aimable.DeactivateAimingOnCooldown) != null)
			{
				uiReceiverList.ForEach(r => ((UIReceiver)r).SetCustomButtonOnCooldown(bindable.BindingIndex, true));
			}
		}

		public static void UpdateBindingIndex(this IBindable bindable, int idx, Entity entity)
		{
			bindable.BindingIndex = idx;

			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			if (dstManager.HasComponent<BindedActionsCooldownData>(entity))
			{
				var existingComponent = dstManager.GetComponentData<BindedActionsCooldownData>(entity);

				existingComponent.ReadyToUseBindingIndexes.Add(idx);
				dstManager.SetComponentData(entity, existingComponent);
				return;
			}

			World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, new BindedActionsCooldownData
			{
				ReadyToUseBindingIndexes = new FixedList32Bytes<int> { bindable.BindingIndex }
			});
		}
	}
}