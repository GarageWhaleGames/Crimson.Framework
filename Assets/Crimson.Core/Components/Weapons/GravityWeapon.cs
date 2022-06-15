using Assets.Crimson.Core.Common;
using Crimson.Core.Common;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Components.Weapons
{
	public class GravityWeapon :
		MonoBehaviour,
		IWeapon,
		IHasComponentName
	{
		public string ComponentName
		{
			get => componentName;
			set => componentName = value;
		}

		[Space]
		[ShowInInspector]
		[SerializeField]
		public string componentName = "";

		public InputActionReference _activationAction;
		public Entity _entity;
		public float _maxDistance = 10;
		public float _boxSize = 1.5f;
		public Vector3 MagnetOffset = Vector3.forward;

		public ActionsList ActionsOnEnable;
		public ActionsList ActionsOnDisable;

		private EntityManager _entityManager;
		[SerializeField] private float _force = 10;
		private bool _isEnable;
		private RaycastHit[] _raycastResults = new RaycastHit[100];

		public event System.Action OnShot;

		public IActor Actor { get; set; }

		public bool IsEnable
		{
			get => _isEnable;
			set
			{
				_isEnable = value;
				if (_isEnable)
				{
					ActionsOnEnable.Execute();
				}
				else
				{
					ActionsOnDisable.Execute();
					Deactivate();
				}
			}
		}

		public Vector3 MagnetPoint => transform.TransformPoint(MagnetOffset);

		public void Activate()
		{
			SetActivateState(true);
		}

		private void Awake()
		{
			ActionsOnDisable.Init();
			ActionsOnEnable.Init();
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;

			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var data = new MagnetPointData()
			{
				IsActive = false,
				Offset = MagnetOffset,
				Force = _force,
			};
			_entityManager.AddComponentData(_entity, data);

			_activationAction.action.performed += ActivateActionHandler;
		}

		public void Deactivate()
		{
			SetActivateState(false);
		}

		public void Execute()
		{
		}

		public void Reload()
		{
		}

		public void StartFire()
		{
			Activate();
			TryGrabObject();
		}

		public void StopFire()
		{
			Deactivate();
		}

		private void ActivateActionHandler(InputAction.CallbackContext obj)
		{
			if (!IsEnable)
			{
				return;
			}
			Execute();
		}

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireSphere(MagnetOffset, 1);
			var center = Vector3.forward * (_maxDistance / 2);
			var halfExtents = _boxSize * Vector3.one;
			halfExtents.z = _maxDistance;
			Gizmos.DrawWireCube(center, halfExtents);
		}

#endif

		private void SetActivateState(bool state)
		{
			if (_entity == Entity.Null || !_entityManager.HasComponent<MagnetPointData>(_entity))
			{
				return;
			}
			var pointData = _entityManager.GetComponentData<MagnetPointData>(_entity);
			pointData.IsActive = state;
			_entityManager.SetComponentData(_entity, pointData);
		}

		private void TryGrabObject()
		{
			OnShot?.Invoke();
			var center = transform.position;
			center += transform.forward.normalized * (_maxDistance / 2);
			var halfExtents = _boxSize * Vector3.one;
			halfExtents.z = _maxDistance;
			halfExtents /= 2;
			var resultsCount = Physics.BoxCastNonAlloc(center, halfExtents, transform.forward, _raycastResults);
			if (resultsCount == 0)
			{
				return;
			}

			for (var i = 0; i < resultsCount; i++)
			{
				var result = _raycastResults[resultsCount - 1];
				if (result.rigidbody == null)
				{
					continue;
				}
				var ability = result.rigidbody.GetComponent<AbilityMagnet>();
				if (ability == null)
				{
					continue;
				}
				ability.MagnetTo(_entity);
			}
		}
	}
}