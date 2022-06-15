using Assets.Crimson.Core.AI.AimLaserFX;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components
{
	public class AbilitySearchingLaser : MonoBehaviour, IActorAbility
	{
		private SearchingLaser _laser;
		public float Angle = 5;
		public float Duratuion = 1;
		public LaserStateType State;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			Actor = actor;
			_laser = new SearchingLaser(Angle, Duratuion)
			{
				Target = transform
			};
		}

		[Button]
		public void Execute()
		{
			_laser.StateType = State;
		}
	}
}