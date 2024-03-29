﻿using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Types;
using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Linq;
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
		public InputActionReference _activationAction;

		public Entity _entity;

		public float _maxDistance = 10;

		public WeaponType _weaponType;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public MonoBehaviour abilityOnShot;

		[Header("ActionsOnDisable")]
		public ActionsList ActionsOnDisable = new ActionsList();

		[Header("ActionsOnEnable")]
		public ActionsList ActionsOnEnable = new ActionsList();

		public string AnimationIn;

		public string AnimationOut;

		[Space]
		[ShowInInspector]
		[SerializeField]
		public string componentName = "";

		public Animator DistortionAnimator;

		public Vector3 MagnetOffset = Vector3.forward;

		public GameObject[] WeaponReadyFXReferences;

		private EntityManager _entityManager;

		[SerializeField] private float _force = 10;

		[SerializeField] private GameObject _grabEffect;

		private bool _isEnable;

		private RaycastHit[] _raycastResults = new RaycastHit[25];

		public event System.Action OnShot;

		public IActor Actor { get; set; }

		public string ComponentName
		{
			get => componentName;
			set => componentName = value;
		}

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
					IsReady = false;
					ActionsOnDisable.Execute();
					Deactivate();
				}
			}
		}

		public bool IsReady
		{
			get => WeaponReadyFXReferences.All(s => s.activeSelf);
			set => WeaponReadyFXReferences.ForEach(s => s.SetActive(value));
		}

		public Vector3 MagnetPoint => transform.TransformPoint(MagnetOffset);

		public WeaponType Type => _weaponType;

		private bool IsInvalidActor => _entity == Entity.Null
			|| _entityManager == null
			|| _entityManager.HasComponent<DeadActorTag>(_entity)
			|| _entityManager.HasComponent<StopInputTag>(_entity);

		public void Activate()
		{
			SetActivateState(true);
		}

		public void AddAmmo(IAmmo ammo)
		{
			Debug.Log("Gravity weapon has not ammo logic now");
		}

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			IsReady = false;
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
			if (IsInvalidActor)
			{
				return;
			}
			Activate();
			TryGrabObject();
		}

		public void StopFire()
		{
			if (IsInvalidActor)
			{
				return;
			}
			if (abilityOnShot != null) ((IActorAbility)abilityOnShot).Execute();

			Deactivate();
		}

		private void ActivateActionHandler(InputAction.CallbackContext obj)
		{
			if (!IsEnable || !IsInvalidActor)
			{
				return;
			}
			Execute();
		}

		private void Awake()
		{
			ActionsOnDisable.Init();
			ActionsOnEnable.Init();
		}

#if UNITY_EDITOR

		private bool MustBeAbility(MonoBehaviour a)
		{
			return (a is IActorAbility) || (a is null);
		}

		private bool MustBeAimable(MonoBehaviour behaviour)
		{
			return behaviour is IActorAbility;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireSphere(MagnetOffset, 1);
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
			if (_grabEffect != null)
				_grabEffect.SetActive(state);
			if (DistortionAnimator != null)
			{
				string animation = state ? AnimationIn : AnimationOut;
				if (!string.IsNullOrEmpty(animation))
					DistortionAnimator.Play(animation, 0, 0);
			}
		}

		private void TryGrabObject()
		{
			OnShot?.Invoke();
			//TODO: Need ecs and controller support
			//FIXME:
			var mousePosition = Input.mousePosition;
			var ray = Camera.main.ScreenPointToRay(mousePosition);
			var resultsCount = Physics.RaycastNonAlloc(ray, _raycastResults, _maxDistance);
			if (resultsCount == 0)
			{
				return;
			}
			var result = _raycastResults[resultsCount - 1];
			var target = result.rigidbody;
			if (target == null)
			{
				return;
			}
			var abilityMagnet = target.GetComponent<AbilityMagnet>();
			if (abilityMagnet != null)
			{
				abilityMagnet.MagnetTo(_entity);
			}
		}
	}
}