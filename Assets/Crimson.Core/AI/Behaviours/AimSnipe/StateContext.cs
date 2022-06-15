using Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe
{
	internal class StateContext : IStateContext
	{
		public IState Current { get; private set; }

		public void TransitionTo(IState state)
		{
			if (Current != null)
			{
				Current.Kill();
			}
			Current = state;
			state.SetContext(this);
		}

		public void Clear()
		{
			Current = null;
		}

		public void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData)
		{
			Current.Handle(entity, dstManager, ref inputData);
		}
	}
}