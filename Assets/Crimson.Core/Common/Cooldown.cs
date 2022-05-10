using Sirenix.OdinInspector;

namespace Assets.Crimson.Core.Common
{
	[System.Serializable, HideLabel]
	public struct Cooldown
	{
		public bool Use;

		[ShowIf(nameof(Use))]
		public float Duration;

		private float _lastAttackTime;

		public bool IsExpired => !Use || UnityEngine.Time.realtimeSinceStartup - _lastAttackTime > Duration;

		public void Reset()
		{
			if (!Use)
			{
				return;
			}

			_lastAttackTime = UnityEngine.Time.realtimeSinceStartup;
		}
	}
}