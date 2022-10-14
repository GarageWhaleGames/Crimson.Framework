using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Plugins.Behavior_Designer.Behavior_Designer_Tactical.Scripts.Tasks
{
	[TaskCategory("Tactical")]
	[TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}AttackIcon.png")]
	public class AttackTarget : Action
	{
		public SharedGameObject Target;
		public SharedInt AttackInput = 1;
		private Actor _actor;
		private EntityManager _entityManager;

		public override void OnAwake()
		{
			_actor = GetComponent<Actor>();
			_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			base.OnAwake();
		}

		public override TaskStatus OnUpdate()
		{
			if (Target.Value == null)
			{
				return TaskStatus.Failure;
			}

			var data = _entityManager.GetComponentData<PlayerInputData>(_actor.ActorEntity);
			data.CustomInput[AttackInput.Value] = 1f;
			_entityManager.SetComponentData(_actor.ActorEntity, data);
			return TaskStatus.Success;
		}
	}
}