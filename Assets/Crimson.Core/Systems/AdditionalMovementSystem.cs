using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Systems
{
	public class AdditionalMovementSystem : ComponentSystem
	{
		private EntityQuery _directAgentQuery;
		private EntityQuery _directTranformQuery;
		private EntityQuery _moveAgentQuery;
		private EntityQuery _moveTransformQuery;

		protected override void OnCreate()
		{
			_moveTransformQuery = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.Exclude<NavMeshAgent>(),
				ComponentType.ReadWrite<MoveData>());

			_moveAgentQuery = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<NavMeshAgent>(),
				ComponentType.ReadWrite<MoveData>());

			_directTranformQuery = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<AbilityTargetDirectionMove>(),
				ComponentType.Exclude<NavMeshAgent>(),
				ComponentType.ReadWrite<DirectMoveData>());

			_directAgentQuery = GetEntityQuery(
				ComponentType.ReadOnly<Transform>(),
				ComponentType.ReadOnly<AbilityTargetDirectionMove>(),
				ComponentType.ReadOnly<NavMeshAgent>(),
				ComponentType.ReadWrite<DirectMoveData>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_moveTransformQuery).ForEach(
				(Entity entity, Transform transform, ref MoveData moveData) =>
				{
					var direction = (Vector3)moveData.EndPosition - transform.position;
					var ray = new Ray(transform.position, direction);
					var results = new RaycastHit[1];
					var positionThreshold = Mathf.Max(moveData.PositionThreshold, 0.1f);
					var nextPosition = direction.normalized * moveData.Velocity * UnityEngine.Time.deltaTime;
					if ((!moveData.IgnoreRaycast && Physics.RaycastNonAlloc(ray, results, nextPosition.magnitude) > 0)
					 || direction.magnitude <= positionThreshold)
					{
						EntityManager.RemoveComponent<MoveData>(entity);
						return;
					}
					transform.position += nextPosition;
				});

			Entities.With(_moveAgentQuery).ForEach(
				(Entity entity, NavMeshAgent agent, ref MoveData moveData) =>
				{
					agent.enabled = false;
					var direction = (Vector3)moveData.EndPosition - agent.transform.position;
					var positionThreshold = Mathf.Max(moveData.PositionThreshold, 0.1f);
					var offsetVector = direction.normalized * -1 * positionThreshold;
					direction += offsetVector;
					var ray = new Ray(agent.transform.position, direction);
					var results = new RaycastHit[1];
					var nextPosition = direction.normalized * moveData.Velocity * UnityEngine.Time.deltaTime;
					if ((!moveData.IgnoreRaycast && Physics.RaycastNonAlloc(ray, results, nextPosition.magnitude) > 0)
					 || direction.magnitude <= positionThreshold)
					{
						EntityManager.RemoveComponent<MoveData>(entity);
						agent.enabled = true;
						return;
					}

					agent.transform.position += nextPosition;
				});

			Entities.With(_directTranformQuery).ForEach(
				(Entity entity, Transform transform, ref DirectMoveData data) =>
				{
					if (data.Duration <= 0)
					{
						EntityManager.RemoveComponent<DirectMoveData>(entity);
						return;
					}
					var deltaTime = UnityEngine.Time.deltaTime;
					data.Duration -= deltaTime;
					var direction = (Vector3)data.Direction;
					var lookDirection = transform.position + direction;
					lookDirection.y = transform.position.y;
					transform.LookAt(lookDirection);

					transform.position += direction.normalized * data.Velocity * deltaTime;
				});

			Entities.With(_directAgentQuery).ForEach(
				(Entity entity, ref DirectMoveData data, NavMeshAgent agent, AbilityTargetDirectionMove ability) =>
				{
					if ((ability.PreFXInstance == null || ability.PreFXInstance.Count == 0)
						&& ability.PreFX.Count != 0)
					{
						agent.ResetPath();
						ability.PreFXInstance = ability.SpawnFX(ability.PreFX);
					}

					if (data.Duration <= 0)
					{
						EntityManager.RemoveComponent<DirectMoveData>(entity);
						ability.DestroyFX(ability.PreFXInstance);
						ability.PreFXInstance = null;
						ability.SpawnFX(ability.PostFX);
						return;
					}

					var transform = agent.transform;
					var deltaTime = UnityEngine.Time.deltaTime;
					data.Duration -= deltaTime;
					var direction = (Vector3)data.Direction;
					var lookDirection = transform.position + direction;
					lookDirection.y = transform.position.y;
					transform.LookAt(lookDirection);
					var newPosition = direction.normalized * data.Velocity * deltaTime;
					var path = new NavMeshPath();
					if (NavMesh.CalculatePath(transform.position, newPosition, NavMesh.AllAreas, path))
					{
						agent.Move(newPosition);
					}
					else
					{
						ability.DestroyFX(ability.PreFXInstance);
						ability.PreFXInstance = null;
						ability.SpawnFX(ability.PostFX);
						EntityManager.RemoveComponent<DirectMoveData>(entity);
						return;
					}
				});
		}
	}
}