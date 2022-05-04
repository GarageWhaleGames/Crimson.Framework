using Assets.Crimson.Core.Loading.SpawnDataTypes;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Loading.SpawnDataTypes;
using Crimson.Core.Utils;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Crimson.Core.Common
{
	public class ActorSpawn
	{
		[CanBeNull]
		public static List<SpawnItemData> GenerateData(IActorSpawnerSettings settings, IActor spawner = null,
			IActor owner = null)
		{
			if (settings.SpawnerDisabled)
			{
				return new List<SpawnItemData>();
			}

			int spawnCount;

			var sampledComponents = new List<Component>();

			if (settings.SpawnPosition == SpawnPosition.UseSpawnPoints &&
				settings.SpawnPointsFrom == SpawnPointsSource.FindByTag)
			{
				settings.SpawnPoints = GameObject.FindGameObjectsWithTag(settings.SpawnPointTag).ToList();
				if (settings.SpawnPoints.Count == 0)
				{
					Debug.LogError("[LEVEL ACTOR SPAWNER] No spawn points found with tag: " +
								   settings.SpawnPointTag + ". Aborting!");
					return null;
				}
			}

			if (settings.SpawnPosition == SpawnPosition.UseSpawnPoints && settings.UseChildrenObjects)
			{
				var newSpawnPoints = new List<GameObject>();

				for (var i = 0; i < settings.SpawnPoints.Count; i++)
				{
					var point = settings.SpawnPoints[i];
					for (var j = 0; j < point.transform.childCount; j++)
					{
						newSpawnPoints.Add(point.transform.GetChild(j).gameObject);
					}
				}

				settings.SpawnPoints = newSpawnPoints;
			}

			switch (settings.FillSpawnPoints)
			{
				case FillMode.UseEachObjectOnce:
					spawnCount = settings.ObjectsToSpawn.Count;
					break;

				case FillMode.FillAllSpawnPoints:
					spawnCount = settings.SpawnPoints.Count;
					break;

				case FillMode.PlaceEachObjectXTimes:
					spawnCount = settings.ObjectsToSpawn.Count * settings.X;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			if (settings.SkipBusySpawnPoints && settings.SpawnPosition == SpawnPosition.UseSpawnPoints)
			{
				for (var i = 0; i < settings.SpawnPoints.Count; i++)
				{
					var actorSpawnedOnPoint = settings.SpawnPoints[i].GetComponent<ActorSpawnedOnPoint>();

					if (actorSpawnedOnPoint != null && actorSpawnedOnPoint.actor != null)
					{
						settings.SpawnPoints.RemoveAt(i);
						spawnCount--;
						i--;
					}
				}
			}

			if (settings.SpawnPointsFillingMode == FillOrder.RandomOrder)
			{
				settings.ObjectsToSpawn =
					settings.ObjectsToSpawn.OrderBy(item => settings.Rnd.Next()).ToList();
				settings.SpawnPoints =
					settings.SpawnPoints.OrderBy(item => settings.Rnd.Next()).ToList();
			}

			var results = new List<SpawnItemData>(spawnCount);

			for (var i = 0; i < spawnCount; i++)
			{
				var data = new SpawnItemData
				{
					Position = Vector3.zero,
					Rotation = Quaternion.identity
				};
				switch (settings.SpawnPosition)
				{
					case SpawnPosition.UseSpawnPoints:
					{
						if (settings.SpawnPoints.Count == 0)
						{
							Debug.LogError($"[ACTOR SPAWNER] In Use Spawn Points mode you have to provide some spawning points! \n" +
								$"Spawner is {spawner}, and object is {settings.ObjectsToSpawn[0]}");
						}
						var point = settings.SpawnPoints[i % settings.SpawnPoints.Count];
						data.Position = point.transform.position;
						if (settings.RotationOfSpawns == RotationOfSpawns.UseSpawnPointRotation)
						{
							data.Rotation = point.transform.rotation;
						}

						break;
					}

					case SpawnPosition.RandomPositionOnNavMesh:
						data.Position = NavMeshRandomPointUtil.GetRandomLocation();
						break;

					case SpawnPosition.UseSpawnerPosition:
						if (spawner == null || spawner.GameObject == null)
						{
							Debug.LogError("[ACTOR SPAWNER] You are using Use Spawner Position, but Spawner is NULL!");
							continue;
						}

						data.Position = spawner.GameObject.transform.position;
						break;

					case SpawnPosition.UseZeroPosition:
						data.Position = Vector3.zero;
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				switch (settings.RotationOfSpawns)
				{
					case RotationOfSpawns.UseRandomRotationY:
						data.Rotation = Quaternion.Euler(0f, settings.Rnd.Next() % 360f, 0f);
						break;

					case RotationOfSpawns.UseRandomRotationXYZ:
						data.Rotation = Quaternion.Euler(settings.Rnd.Next() % 360f, settings.Rnd.Next() % 360f,
							settings.Rnd.Next() % 360f);
						break;

					case RotationOfSpawns.UseSpawnPointRotation:
						if (settings.SpawnPosition == SpawnPosition.UseSpawnerPosition)
						{
							data.Rotation = spawner.GameObject.transform.rotation;
						}

						break;

					case RotationOfSpawns.UseZeroRotation:
						data.Rotation = Quaternion.Euler(Vector3.zero);
						break;

					case RotationOfSpawns.SpawnerMovement:
						try
						{
							var spawnerMovementData =
								World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<ActorMovementData>(
									spawner.ActorEntity);
							data.Rotation = spawnerMovementData.Input.Equals(float3.zero) ? Quaternion.Euler(Vector3.zero) : Quaternion.LookRotation(Vector3.Normalize(spawnerMovementData.Input));
						}
						catch
						{
							Debug.LogError(
								"[ACTOR SPAWNER] To get Rotation from Spawner Movement, you need IActor Spawner and ActorMovementData on it!");
							data.Rotation = Quaternion.Euler(Vector3.zero);
						}

						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				if (settings.CopyComponentsFromSamples.Count > 0)
				{
					sampledComponents = new List<Component>();

					foreach (var sample in settings.CopyComponentsFromSamples)
					{
						if (sample == null) continue;

						foreach (var component in sample.GetComponents<Component>())
						{
							if (component == null || component is Transform || component is IActor) continue;
							switch (settings.CopyComponentsOfType)
							{
								case ComponentsOfType.OnlyAbilities when !(component is IActorAbility):
									continue;
								case ComponentsOfType.OnlySimpleBehaviours when component is IActorAbility:
									continue;
								case ComponentsOfType.AllComponents:
								default:
									sampledComponents.Add(component);
									break;
							}
						}
					}

					if (sampledComponents.Count == 0)
						Debug.LogError("[LEVEL ACTOR SPAWNER] No suitable components found in sample game objects!");
				}

				var index = i % settings.ObjectsToSpawn.Count;
				data.Prefab = settings.ObjectsToSpawn[index];
				data.SampledComponents = sampledComponents.ToArray();
				data.Spawner = spawner;
				data.Owner = owner;
				data.DeleteExistingComponents = settings.DeleteExistingComponents;

				if (settings.ParentOfSpawns != TargetType.None)
				{
					var parentData = new SpawnParentData
					{
						Target = settings.ParentOfSpawns,
						ComponentName = settings.ActorWithComponentName,
						Tag = settings.ParentTag,
						TargetStrategy = settings.ChooseStrategy
					};

					data.Parent = parentData;
				}

				results.Add(data);
			}

			return results;
		}

		public static List<GameObject> Spawn(IActorSpawnerSettings spawnSettings, IActor spawner = null,
							IActor owner = null)
		{
			var spawnItems = GenerateData(spawnSettings, spawner, owner);
			if (spawnItems == null) return new List<GameObject>();
			var results = new List<GameObject>();
			for (var i = 0; i < spawnItems.Count; i++)
			{
				results.Add(Spawn(spawnItems[i]));
			}
			return results;
		}

		public static GameObject Spawn(SpawnItemData data)
		{
			var result = data.Prefab.CreateFromPrefab();
			result.transform.SetPositionAndRotation(data.Position, data.Rotation);

			var actors = result.GetComponents<IActor>();

			if (!data.Parent.IsEmpty)
			{
				var parents = FindActorsUtils.GetActorsList(result, data.Spawner, data.Parent.Target,
					   data.Parent.ComponentName, data.Parent.Tag, data.Parent.TargetStrategy == ChooseTargetStrategy.FirstInChildren);

				var parent = FindActorsUtils.ChooseActor(result.transform, parents, data.Parent.TargetStrategy);

				result.transform.SetParent(parent);
			}

			if (data.SampledComponents != null && data.SampledComponents.Length > 0)
			{
				if (data.DeleteExistingComponents)
				{
					foreach (var component in result.GetComponents<Component>())
					{
						if (component is Transform || component is IActor)
						{
							continue;
						}

						Object.Destroy(component);
					}
				}

				result.CopyComponentsWithLinks(data.SampledComponents);
			}

			if (actors.Length > 1)
			{
				Debug.LogError("[ACTOR SPAWNER] Only one IActor Component for Actor allowed!");
			}
			else if (actors.Length == 1)
			{
				var targetActor = actors[0];
				if (data.Spawner != null)
				{
					data.Spawner.ChildrenSpawned++;
					targetActor.ActorId = data.Spawner.ActorId;
				}

				targetActor.Spawner = data.Spawner;
				targetActor.Owner = data.Owner ?? data.Spawner ?? targetActor;
				targetActor.PerformSpawnActions();
			}

			if (data.SpawnPoint != null)
			{
				data.SpawnPoint.AddComponent<ActorSpawnedOnPoint>().actor = result;
			}

			return result;
		}

		internal static void SimpleSpawn(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			var spawnData = new SpawnItemData()
			{
				Prefab = prefab,
				Position = position,
				Rotation = rotation,
				SampledComponents = new Component[0]
			};
			Spawn(spawnData);
		}
	}
}