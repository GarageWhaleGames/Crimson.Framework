using Crimson.Core.Components;
using Crimson.Core.Utils;
using Crimson.Core.Utils.LowLevel;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Systems
{
	[UpdateInGroup(typeof(FixedUpdateGroup))]
	public class PlayerMovementSystem : JobComponentSystem
	{
		[BurstCompile]
		private struct PlayerMovementJob : IJobForEach<PlayerInputData, ActorMovementData>
#pragma warning restore 618
		{
			public void Execute(ref PlayerInputData input, ref ActorMovementData movement)
			{
				var inputVector = MathUtils.RotateVector(input.Move, 0 - input.CompensateAngle);
				var tempInput = inputVector;
				if (input.MinMagnitude == 0f)
				{
					movement.Input = new float3(inputVector.x, 0f, inputVector.y);
				}
				else
				{
					if ((math.lengthsq(inputVector) > 0) && (math.lengthsq(inputVector) < (input.MinMagnitude * input.MinMagnitude)))
					{
						tempInput = math.normalize(inputVector) * input.MinMagnitude;
					}
					movement.Input = new float3(tempInput.x, 0f, tempInput.y);
				}
			}
		}

		[BurstCompile]
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new PlayerMovementJob();
			return job.Schedule(this, inputDeps);
		}
	}
}