using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Common.Types;
using Crimson.Core.Common;
using Crimson.Core.Components;
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

		public WeaponType _weaponType;

		public InputActionReference _activationAction;
		public Entity _entity;
		public float _maxDistance = 10;
		public Vector3 MagnetOffset = Vector3.forward;

		[Header("ActionsOnEnable")]
		public ActionsList ActionsOnEnable = new ActionsList();

		[Header("ActionsOnDisable")]
		public ActionsList ActionsOnDisable = new ActionsList();

		private EntityManager _entityManager;
		[SerializeField] private float _force = 10;

		[SerializeField] private GameObject _grabEffect;

		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public MonoBehaviour abilityOnShot;

		public Animator DistortionAnimator;
		public string AnimationIn;
		public string AnimationOut;

		private bool _isEnable;
		private RaycastHit[] _raycastResults = new RaycastHit[25];

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

		public WeaponType Type => _weaponType;

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
			if (abilityOnShot != null) ((IActorAbility)abilityOnShot).Execute();

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
		}

		private bool MustBeAimable(MonoBehaviour behaviour)
		{
			return behaviour is IActorAbility;
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

		private bool MustBeAbility(MonoBehaviour a)
		{
			return (a is IActorAbility) || (a is null);
		}
	}
}