using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	//[UpdateAfter(typeof(UserInputSystem))]
	[UpdateBefore(typeof(PlayerMovementSystem))]
	public class InputCompensateCameraRotationSystem : ComponentSystem
	{
		private Transform _camera;

		protected override void OnUpdate()
		{
			if (_camera == null)
			{
				var c = Camera.main;
				if (c == null) return;

				_camera = c.transform;
			}
			else if (_camera != Camera.main.transform)
			{
				_camera = Camera.main.transform;
			}

			Entities.WithAll<CompensateCameraRotation, PlayerInputData>().ForEach(
				(Entity entity, ref PlayerInputData input) =>
				{
					input.CompensateAngle = _camera.rotation.eulerAngles.y;
				});
		}
	}
}