using Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe
{
	internal interface IStateContext
	{
		IState Current { get; }

		void TransitionTo(IState state);

		void Clear();

		void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData);
	}
}