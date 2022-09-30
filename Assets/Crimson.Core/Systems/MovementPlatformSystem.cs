using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Extensions;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Crimson.Core.Systems
{
	public class MovementPlatformSystem : ComponentSystem
	{
		private EntityQuery _platformsQuery;
		private Collider[] _results = new Collider[Constants.COLLISION_BUFFER_CAPACITY];
  
		protected override void OnCreate()
		{
			_platformsQuery = GetEntityQuery(
				ComponentType.ReadWrite<MovingPlatformData>(),
				ComponentType.ReadOnly<ActorColliderData>());
		}

		protected override void OnUpdate()
		{
			Entities.With(_platformsQuery).ForEach(
			(Transform transform, ref ActorColliderData colliderData, ref MovingPlatformData platformData) =>
			{
				float3 entityPosition = transform.position;
				quaternion entityRotation = transform.rotation;
				Vector3 positionDelta = entityPosition - platformData.Position;
				var rotationDelta = entityRotation.value - platformData.Rotation.value;

				platformData.Position = entityPosition;
				platformData.Rotation = entityRotation;

				if (math.length(positionDelta) == 0)
				{
					return;
				}

				var size = colliderData.GetResults(transform, _results);

				for (var i = 0; i < size; i++)
				{
					var result = _results[i];
					var hasActor = result.GetComponent<Actor>();
					if (!hasActor || result.transform == transform)
					{
						continue;
					}
					result.transform.position += positionDelta;
					result.transform.rotation *= new quaternion(rotationDelta);
					var agent = result.GetComponent<NavMeshAgent>();
					if (agent && agent.isOnNavMesh)
					{
						agent.ResetPath();
					}
				}
			});
		}
	}
}