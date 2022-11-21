using BehaviorDesigner.Runtime.Tasks;
using Crimson.Core.Common;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Reflection;
using Unity.Entities;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class HasComponentData : Conditional
	{
		[ValueDropdown(nameof(GetComponentTypes))]
		public string ComponentName;

		private Actor _actor;
		private Type _checkType;
		private EntityManager _entityManager;
		private Type[] Types => Assembly.GetAssembly(typeof(IActor)).GetTypes();

		public override void OnAwake()
		{
			_actor = GetComponent<Actor>();
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!string.IsNullOrEmpty(ComponentName))
			{
				_checkType = Types.First(s => s.Name.Contains(ComponentName));
			}
		}

		public override TaskStatus OnUpdate()
		{
			var state = _entityManager.HasComponent(_actor.ActorEntity, _checkType);
			return state ? TaskStatus.Success : TaskStatus.Failure;
		}

		private string[] GetComponentTypes()
		{
			return Types
				.Where(s => s.GetInterfaces().Contains(typeof(IComponentData)))
				.Select(s => s.Name)
				.ToArray();
		}
	}
}