using Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe
{
	internal abstract class State : IState
	{
		protected IStateContext _context;

		public State()
		{
			IsDone = false;
		}

		public bool IsDone { get; protected set; }

		public void SetContext(IStateContext context)
		{
			_context = context;
		}

		public abstract void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData);

		public virtual void Kill()
		{
		}
	}
}