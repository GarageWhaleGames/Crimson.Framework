using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Perks
{
	public class PerkScanSector : MonoBehaviour, IPerkAbility, IActorAbility
	{
		public float AngleSpeed;

		[MinMaxSlider(-180, 180, true)]
		public Vector2 Sector = new Vector2(-180, 180);

		private Entity _entity;

		private EntityManager _entityManager;
		private Vector3 _initDirection;
		private Quaternion _initRotation;
		private bool _invertAngle;
		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_initRotation = transform.rotation;
			_initDirection = transform.forward;

			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		public void Apply(IActor target)
		{
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
		}

		[Button]
		public void Execute()
		{
			var velocity = AngleSpeed * Time.deltaTime;
			if (_invertAngle)
			{
				velocity *= -1;
			}
			var angle = Vector3.SignedAngle(_initDirection, transform.forward, transform.up);
			var targetAngle = angle + velocity;
			var clampedAngle = Mathf.Clamp(targetAngle, Sector.x, Sector.y);
			var currentAngle = velocity - (targetAngle - clampedAngle);
			transform.rotation *= Quaternion.AngleAxis(currentAngle, transform.up);
			var isMinimalLimit = clampedAngle <= Sector.x;
			var isMaximumLimit = clampedAngle >= Sector.y;
			if (isMinimalLimit || isMaximumLimit)
			{
				_invertAngle = !_invertAngle;
			}
		}

		public void Remove()
		{
			if (Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Remove(this);
			}
			Destroy(this);
		}

		private void OnDrawGizmosSelected()
		{
			Handles.color = Color.magenta;
			Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, Sector.x, 2);
			Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, Sector.y, 2);
		}
	}
}