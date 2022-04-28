using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilityDestroyObject : MonoBehaviour, IActorAbility
	{
		private Entity _entity;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			_entity = entity;
			Actor = actor;
		}

		public void Execute()
		{
			this.gameObject.DestroyWithEntity(_entity);
		}
	}
}