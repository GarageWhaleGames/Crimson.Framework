using Assets.Crimson.Core.Common.Magnets;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Systems.Magnets
{
	public class MagnetSnappingSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAll<MagnetSnapData>().ForEach(
				(Entity entity, Rigidbody rigidbody, ref MagnetSnapData snapData) =>
				{
					var targetTransform = EntityManager.GetComponentObject<Transform>(snapData.Target);

					rigidbody.isKinematic = true;
					rigidbody.useGravity = false;
					var distance = Vector3.Distance(targetTransform.position, rigidbody.position);
					var angle = Quaternion.Angle(targetTransform.rotation, rigidbody.rotation);

					var needPositionLerp = distance > snapData.DistanceThreshold;
					if (needPositionLerp)
					{
						rigidbody.position = Vector3.Lerp(rigidbody.position,
										targetTransform.position,
										Time.DeltaTime * snapData.SnapSpeed);
					}

					var needRotationLerp = angle > snapData.RotationTheshold;
					if (needRotationLerp)
					{
						rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation,
										   targetTransform.rotation,
										   Time.DeltaTime * snapData.SnapSpeed);
					}

					if (!needPositionLerp && !needRotationLerp)
					{
						rigidbody.isKinematic = false;
						rigidbody.useGravity = true;

						rigidbody.position = targetTransform.position;
						rigidbody.rotation = targetTransform.rotation;

						EntityManager.RemoveComponent<MagnetSnapData>(entity);
					}
				});
		}
	}
}