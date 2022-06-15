using Crimson.Core.Components;
using Unity.Entities;

namespace Assets.Crimson.Core.AI.Behaviours.AimSnipe
{
	internal interface IState
	{
		bool IsDone { get; }

		void SetContext(IStateContext stateContext);

		void Handle(Entity entity, EntityManager dstManager, ref PlayerInputData inputData);

		void Kill();
	}
}