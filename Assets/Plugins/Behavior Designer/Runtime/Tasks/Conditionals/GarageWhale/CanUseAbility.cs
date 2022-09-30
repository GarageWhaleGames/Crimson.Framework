using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Crimson.Core.Components;
using Crimson.Core.Utils;
using System.Collections.Generic;

namespace Assets.Plugins.Behavior_Designer.Runtime.Tasks.Conditionals.GarageWhale
{
	[TaskCategory("GarageWhale")]
	public class CanUseAbility : Conditional
	{
		public SharedInt AttackInput = 1;
		private AbilityPlayerInput _input;
		private List<IActorAbility> _actions;

		public override void OnStart()
		{
			_input = GetComponent<AbilityPlayerInput>();
		}

		public override TaskStatus OnUpdate()
		{
			if (_input.bindingsDict.TryGetValue(AttackInput.Value, out _actions))
			{
				var canUse = _actions.ActionPossible();
				return canUse ? TaskStatus.Success : TaskStatus.Failure;
			}

			return TaskStatus.Failure;
		}
	}
}