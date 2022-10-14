using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;

namespace Assets.Plugins.Behavior_Designer.Behavior_Designer_Tactical.Scripts.Tasks
{
	[TaskCategory("Variables")]
	public class ChangeBool : Action
	{
		[PropertyOrder(1)] public bool Value;
		[PropertyOrder(0)] public SharedBool Variable;

		public override void OnAwake()
		{
		}

		public override TaskStatus OnUpdate()
		{
			Variable.SetValue(Value);
			return TaskStatus.Success;
		}
	}
}