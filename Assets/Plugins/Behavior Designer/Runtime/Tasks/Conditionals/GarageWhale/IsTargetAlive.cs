using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor;
using UnityEngine;
using Crimson.Core.Components;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class IsTargetAlive : Conditional
	{
		public SharedGameObject Target;

		private GameObject _target;
		private AbilityActorPlayer _abilityActorPlayer;

		public override TaskStatus OnUpdate()
		{
			if (Target.Value == null)
			{
				return TaskStatus.Failure;
			}
			if (_target != Target.Value || _abilityActorPlayer)
			{
				_target = Target.Value;
				_abilityActorPlayer = Target.Value.GetComponent<AbilityActorPlayer>();
			}
			if (_abilityActorPlayer == null)
			{
				return TaskStatus.Failure;
			}
			bool isAlive = _abilityActorPlayer.CurrentHealth > 0;

			return isAlive ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}