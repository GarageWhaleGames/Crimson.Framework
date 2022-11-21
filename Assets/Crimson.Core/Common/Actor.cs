using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Common
{
	public struct ActorData : IComponentData
	{
		public int ActorId;
		public int StateId;
	}

	[NetworkSimObject]
	public class Actor : MonoBehaviour, IActor, IHasComponentName, IConvertGameObjectToEntity
	{
		[Space][SerializeField] public string componentName = "";

		[Space]
		[ValidateInput("MustBeAbility", "Ability MonoBehaviours must derive from IActorAbility!")]
		public List<MonoBehaviour> ExecuteOnSpawn = new List<MonoBehaviour>();

		private List<IActorAbility> _abilities = new List<IActorAbility>();

		private int _actorId;

		private int _actorStateId;

		private List<IPerkAbility> _appliedPerks = new List<IPerkAbility>();
		[SerializeField] private IActor _spawner;

		public List<IActorAbility> Abilities
		{
			get
			{
				if (_abilities == null || _abilities.Count == 0)
				{
					_abilities = gameObject.Equals(null) || this == null ? new List<IActorAbility>() : GetComponents<IActorAbility>()?.ToList();
				}

				_abilities?.RemoveAll(a => a.Equals(null));
				return _abilities;
			}
			set => _abilities = value;
		}

		[NetworkSimData] public Entity ActorEntity { get; set; }

		[NetworkSimData]
		public int ActorId
		{
			get
			{
				if (_actorId == 0)
				{
					_actorId = Spawner?.ActorId ?? 0;
				}

				return _actorId;
			}
			set => _actorId = value;
		}

		[NetworkSimData]
		public int ActorStateId
		{
			get
			{
				if (_actorStateId != 0)
				{
					return _actorStateId;
				}

				if (ActorId == 0)
				{
					return 0;
				}
				else
				{
					if (Spawner != null)
					{
						_actorStateId = Spawner.ActorStateId + Spawner.ChildrenSpawned + 1;
					}
					else
					{
						_actorStateId = ActorId * 1000;
					}
					return _actorStateId;
				}
			}
		}

		public List<IPerkAbility> AppliedPerks
		{
			get
			{
				_appliedPerks.RemoveAll(a => a.Equals(null));
				return _appliedPerks;
			}
			set => _appliedPerks = value;
		}

		public ushort ChildrenSpawned { get; set; }

		public string ComponentName
		{
			get => componentName;
			set => componentName = value;
		}

		public List<string> ComponentNames { get; } = new List<string>();
		[NetworkSimData] public GameObject GameObject => this != null && gameObject != null ? gameObject : null;
		[NetworkSimData] public IActor Owner { get; set; }

		[NetworkSimData]
		public IActor Spawner
		{
			get => _spawner;
			set => _spawner = value;
		}

		public EntityManager WorldEntityManager { get; set; }

		public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			ActorEntity = entity;
			WorldEntityManager = dstManager;
			WorldEntityManager.AddComponent<NetworkSyncReceive>(ActorEntity);
			if (!ComponentName.Equals(string.Empty)) ComponentNames.Add(this.ComponentName);

			HandleAbilities(entity);
			PostConvert();
		}

		public virtual void HandleAbilities(Entity entity)
		{
			for (var i = 0; i < Abilities.Count; i++)
			{
				var ability = Abilities[i];
				ability.AddComponentData(ref entity, this);
				if (ability is IHasComponentName compName && !compName.ComponentName.Equals(string.Empty))
				{
					ComponentNames.Add(compName.ComponentName);
				}
			}
		}

		public void PerformSpawnActions()
		{
			foreach (var component in ExecuteOnSpawn)
			{
				if (!(component is IActorAbility ability))
				{
					Debug.LogError($"[ACTOR ABILITY EXECUTION] \"{component.name}\" is not an ability!");
					continue;
				}

				ability.Execute();
			}
		}

		public virtual void PostConvert()
		{
			WorldEntityManager.AddComponentData(ActorEntity, new ActorData { ActorId = ActorId, StateId = ActorStateId });

			if (Spawner == null) return;

			if (WorldEntityManager.HasComponent(Spawner.ActorEntity, typeof(NetworkSyncSend)))
			{
				WorldEntityManager.AddComponentData(ActorEntity, new NetworkSyncSend());
			}
		}

		private void Awake()
		{
			if (World.DefaultGameObjectInjectionWorld == null)
			{
				Debug.LogError(
					"[ACTOR CONVERT TO ENTITY] Convert Entity failed because there was no Active World");
				return;
			}

			// Root ConvertToEntity is responsible for converting the whole hierarchy
			if (transform.parent != null && transform.parent.GetComponentInParent<ConvertToEntity>() != null)
				return;

			this.gameObject.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
		}

		private bool MustBeAbility(List<MonoBehaviour> actions)
		{
			foreach (var action in actions)
			{
				if (action is IActorAbility || action is null) continue;

				return false;
			}

			return true;
		}

		private void OnDestroy()
		{
			try
			{
				if (ActorEntity.Index <= WorldEntityManager.EntityCapacity && WorldEntityManager.Exists(ActorEntity))
				{
					WorldEntityManager.DestroyEntity(ActorEntity);
				}
			}
			catch
			{
				// ignored
			}
		}

		private void OnValidate()
		{
			if (GetComponents<Actor>().Length > 1)
			{
				Debug.LogError("On gameobject more than one actors, destroy on of them", gameObject);
			}
		}
	}
}