using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Forces
{
	[HideMonoScript]
	public class AbilityExplosionForce : MonoBehaviour, IActorAbility, IActorAbilityTarget
	{
		public EntityManager _dstManager;
		public Entity _entity;

		[TitleGroup("Params", order: 1)] public SphereCollider Collider;
		public bool ExecuteOnAwake;
		[TitleGroup("Params", order: 1)] public float Force = 25;
		[TitleGroup("Params", order: 1)] public LayerMask Layer;
		[TitleGroup("Params", order: 1)] public ForceMode Mode = ForceMode.Force;
		public IActor AbilityOwnerActor { get; set; }
		public IActor Actor { get; set; }
		public IActor TargetActor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
			_dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!Actor.Abilities.Contains(this))
			{
				Actor.Abilities.Add(this);
			}
			if (ExecuteOnAwake)
			{
				Execute();
			}
		}

		public void Execute()
		{
			var results = Physics.OverlapSphere(Collider.center + transform.position, Collider.radius, Layer);
			Debug.Log($"Resuls {results.Length}");
			for (var i = 0; i < results.Length; i++)
			{
				results[i].attachedRigidbody.AddExplosionForce(Force, transform.position + Collider.center, Collider.radius, 0, Mode);
			}
		}
	}
}