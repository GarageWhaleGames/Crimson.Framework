using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Crimson.Core.Utils.LowLevel;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class ActorCollisionSystem : ComponentSystem
	{
		private EntityQuery _collisionQuery;
		private EntityQuery _networkQuery;
		private EntityQuery _actorsQuery;

		private Collider[] _results = new Collider[Constants.COLLISION_BUFFER_CAPACITY];
		private Dictionary<int, List<int>> _networkCollisions = new Dictionary<int, List<int>>();
		private Dictionary<int, IActor> _localColliders = new Dictionary<int, IActor>();

		protected override void OnCreate()
		{
			_collisionQuery = GetEntityQuery(
				ComponentType.ReadOnly<AbilityCollision>(),
				ComponentType.ReadOnly<ActorColliderData>(),
				ComponentType.ReadOnly<Transform>());
			_networkQuery = GetEntityQuery(ComponentType.ReadOnly<CollisionReceiveData>());
			_actorsQuery = GetEntityQuery(ComponentType.ReadOnly<ActorData>());
		}

		protected override void OnUpdate()
		{
			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_networkCollisions.Clear();
			Entities.With(_networkQuery).ForEach((Entity entity, ref CollisionReceiveData collision) =>
			{
				if (_networkCollisions.ContainsKey(collision.ActorStateId))
				{
					_networkCollisions[collision.ActorStateId].Add(collision.HitStateId);
				}
				else
				{
					_networkCollisions.Add(collision.ActorStateId, new List<int> { collision.HitStateId });
				}

				PostUpdateCommands.DestroyEntity(entity);
			});

			_localColliders.Clear();

			if (_networkCollisions.Count > 0)
			{
				Entities.With(_actorsQuery).ForEach((Entity entity, ref ActorData id) =>
				{
					var actor = dstManager.GetComponentObject<Actor>(entity);
					if (actor != null) _localColliders.Add(id.StateId, actor);
				});
			}

			Entities.With(_collisionQuery).ForEach(
				(Entity entity, AbilityCollision abilityCollision, ref ActorColliderData colliderData) =>
				{
					var gameObject = abilityCollision.gameObject;
					var transform = gameObject.transform;
					float3 position = transform.position;
					var rotation = transform.rotation;
					var destroyAfterActions = false;

					var size = 0;

					switch (colliderData.ColliderType)
					{
						case ColliderType.Sphere:
							size = Physics.OverlapSphereNonAlloc(
								transform.TransformPoint(colliderData.SphereCenter),
								colliderData.SphereRadius, _results);
							break;

						case ColliderType.Capsule:
							var point1 = transform.TransformPoint(colliderData.CapsuleStart);
							var point2 = transform.TransformPoint(colliderData.CapsuleEnd);
							size = Physics.OverlapCapsuleNonAlloc(point1, point2,
								colliderData.CapsuleRadius, _results);
							break;

						case ColliderType.Box:
							size = Physics.OverlapBoxNonAlloc(
								transform.TransformPoint(colliderData.BoxCenter),
								colliderData.BoxHalfExtents, 
								_results,
								colliderData.BoxOrientation * rotation);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					if (size == 0 && !_networkCollisions.ContainsKey(abilityCollision.Actor.ActorStateId))
					{
						abilityCollision.ExistentCollisions.Clear();
						return;
					}

					var selfHit = 0;

					if (abilityCollision.SpawnerColliders == null)
					{
						var spawner = abilityCollision.Actor.Spawner;
						if ((spawner == null) ||
						((abilityCollision.SpawnerColliders = spawner.GameObject.GetAllColliders()) == null))
						{
							abilityCollision.SpawnerColliders = new List<Collider>();
						}
					}

					if (abilityCollision.OwnColliders == null &&
						(abilityCollision.OwnColliders = abilityCollision.gameObject.GetAllColliders()) == null)
						return;

					_networkCollisions.TryGetValue(abilityCollision.Actor.ActorStateId, out var receivedCollisions);
					var networkCollisionActors = new List<IActor>();
					if (receivedCollisions != null)
					{
						foreach (var col in receivedCollisions)
						{
							_localColliders.TryGetValue(col, out var hitActor);
							if (hitActor != null)
							{
								networkCollisionActors.Add(hitActor);
							}
						}
					}

					for (var i = 0; i <= size; i++)
					{
						Collider hit;
						IActor hitActor;
						if (i == size && networkCollisionActors.Count > 0)
						{
							hitActor = networkCollisionActors.First();
							hit = hitActor.GameObject.GetComponent<Collider>();
							i--;
							networkCollisionActors.RemoveAt(0);
						}
						else if (i < size)
						{
							hit = _results[i];
							hitActor = hit.GetComponent<IActor>();
						}
						else
						{
							continue;
						}

						if (abilityCollision.OwnColliders.Count > 0 &&
							abilityCollision.OwnColliders.FirstOrDefault(c => c == hit)) continue;

						if (colliderData.initialTakeOff)
						{
							if (abilityCollision.SpawnerColliders.Count > 0 &&
								abilityCollision.SpawnerColliders.FirstOrDefault(c => c == hit))
							{
								selfHit++;
								continue;
							}
						}

						if (abilityCollision.debugCollisions && Application.isEditor)
						{
 							Debug.Log($"[COLLISION] HIT] {hit.gameObject} into {abilityCollision.Actor.GameObject} and collision exists: {abilityCollision.ExistentCollisions.Contains(hit)}");
						}

						if (!abilityCollision.ExistentCollisions.Exists(c => c == hit))
						{
							abilityCollision.ExistentCollisions.Add(hit);
						}					

						foreach (var action in abilityCollision.collisionActions)
						{
							if (!action.collisionLayerMask.Contains(hit.gameObject.layer)) continue;
							if (action.useTagFilter)
							{
								switch (action.filterMode)
								{
									case TagFilterMode.IncludeOnly:
										if (!action.filterTags.Contains(hit.gameObject.tag)) continue;
										break;

									case TagFilterMode.Exclude:
										if (action.filterTags.Contains(hit.gameObject.tag)) continue;
										break;

									default:
										throw new ArgumentOutOfRangeException();
								}
							}

							if (action.velocityFilter.Use)
							{
								if (hit.attachedRigidbody == null
								|| !action.velocityFilter.Filter(hit.attachedRigidbody.velocity.magnitude))
								{
									continue;
								}
							}

							if (!action.executeOnCollisionWithSpawner &&
								abilityCollision.SpawnerColliders.Contains(hit))
								continue;

							foreach (var a in action.actions.Where(s => s != null))
							{
								switch (a)
								{
									case IActorAbilityTarget exchange:
										exchange.TargetActor = hitActor;
										exchange.AbilityOwnerActor = abilityCollision.Actor.Owner;
										exchange.Execute();
										break;

									case IActorAbility ability:
										ability.Execute();
										break;
								}
							}

							if (action.destroyAfterAction) destroyAfterActions = true;
						}
					}

					if (selfHit == 0) colliderData.initialTakeOff = false;

					if (destroyAfterActions)
					{
						PostUpdateCommands.AddComponent<ImmediateDestructionActorTag>(entity);
					}

					for (var i = abilityCollision.ExistentCollisions.Count - 1; i >= 0; i--)
					{
						var c = abilityCollision.ExistentCollisions[i];
						if (!_results.Contains(c))
						{
							abilityCollision.ExistentCollisions.RemoveAt(i);
						}
					}
				});
		}
	}
}