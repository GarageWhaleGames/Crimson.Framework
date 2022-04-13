using Crimson.Core.Components;
using System;

namespace Assets.Crimson.Core.Components.Weapons
{
	public interface IWeapon : IActorAbility, IEnableable
	{
		void Reload();

		void StartFire();

		void StopFire();

		event Action OnShot;
	}
}