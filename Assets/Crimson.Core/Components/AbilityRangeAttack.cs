using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components.Tags;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityRangeAttack : MonoBehaviour, IActorAbility, IEnableable
	{
		[Header(nameof(Animation))]
		public ActorGeneralAnimProperties Animation = new ActorGeneralAnimProperties();

		[Header(nameof(Cooldown))]
		public bool Use;

		[ShowIf(nameof(Use))]
		public float Duration;

		[Header("Attack actions")]
		[ValidateInput(nameof(MustBeAbility), "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> actions = new List<MonoBehaviour>();

		private EntityManager _dstManager;

		private Entity _entity;
		private bool _enabled;

		[Header("Attack actions")]
		private float _lastAttackTime;

		public IActor Actor { get; set; }
		public List<IActorAbility> AbilityCollection { get; private set; } = new List<IActorAbility>();

		public bool IsExpiredCooldown => !Use || Time.realtimeSinceStartup - _lastAttackTime > Duration;
		public bool IsEnable { get => IsExpiredCooldown; set => _enabled = value; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_entity = entity;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			Init();
		}

		public void Execute()
		{
			if (!IsExpiredCooldown)
			{
				return;
			}

			ResetCooldown();

			if (Animation.HasAnimation)
			{
				_dstManager.AddComponentData(_entity, new AnimationRangeAttackTag(Animation.AnimationName));
			}

			for (var i = 0; i < AbilityCollection.Count; i++)
			{
				AbilityCollection[i].Execute();
			}
		}

		private void ResetCooldown()
		{
			if (!Use)
			{
				return;
			}

			_lastAttackTime = Time.realtimeSinceStartup;
		}

		private void Init()
		{
			AbilityCollection = new List<IActorAbility>();

			foreach (var a in actions.Where(s => s != null))
			{
				switch (a)
				{
					case IActorAbility ability:
						AbilityCollection.Add(ability);
						break;
				}
			}
		}

		private bool MustBeAbility(List<MonoBehaviour> a)
		{
			return !a.Exists(t => !(t is IActorAbility)) || a.Count == 0;
		}
	}
}