﻿using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityAimAssist : MonoBehaviour, IActorAbility
	{
		[Min(1)]
		public float AimRange = 10;

		public float LockRange = 5;

		private Entity _entity;
		private EntityManager _entityManager;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_entityManager.AddComponentData(_entity, new AimData());
		}

		public Vector3 AimPositionByInput(PlayerInputData inputData)
		{
			var position = transform.position;
			var lookLenght = math.length(inputData.Look);
			Vector2 direction;
			if (lookLenght > 0)
			{
				direction = inputData.Look;
			}
			else
			{
				var camera = Camera.main;
				var mousePosition = (Vector2)inputData.Mouse;
				var ownerScreenPosition = (Vector2)camera.WorldToScreenPoint(position);
				var mouseDirection = mousePosition - ownerScreenPosition;
				direction = mouseDirection.normalized;

				var worldAimRangePostion = position + new Vector3(direction.x, 0, direction.y) * AimRange;
				var aimRangeScreenPosition = camera.WorldToScreenPoint(worldAimRangePostion);
				var aimRangeDirection = (Vector2)aimRangeScreenPosition - ownerScreenPosition;
				var range = math.min(mouseDirection.magnitude, aimRangeDirection.magnitude);
				direction *= range / aimRangeDirection.magnitude;
			}
			if (!direction.Equals(float2.zero))
			{
				var vector = new Vector3(direction.x, 0, direction.y) * AimRange;
				vector = Quaternion.AngleAxis(inputData.CompensateAngle, Vector3.up) * vector;
				position += vector;
			}
			return position;
		}

		public void Execute()
		{
		}

		private void DrawAimRange()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, AimRange);
		}

		private void DrawForward()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawLine(Vector3.zero, transform.forward.normalized * AimRange);
			Gizmos.matrix = Matrix4x4.identity;
		}

		private void OnDrawGizmos()
		{
			var position = transform.position;
			DrawForward();
			if (Application.isPlaying)
			{
				var inputData = _entityManager.GetComponentData<PlayerInputData>(_entity);
				var direction = inputData.Look * AimRange;
				var vector = Vector3.zero;
				if (direction.x != 0 || direction.y != 0)
				{
					vector = new Vector3(direction.x, 0, direction.y);
					vector = Quaternion.AngleAxis(inputData.CompensateAngle, Vector3.up) * vector;
				}
				position += vector;
				Gizmos.DrawLine(transform.position, position);
				Gizmos.color = Color.green;
				var aimData = _entityManager.GetComponentData<AimData>(_entity);
				Gizmos.DrawWireSphere(aimData.LockedPosition, .5f);
			}
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(position, LockRange);
			DrawAimRange();
		}
	}
}