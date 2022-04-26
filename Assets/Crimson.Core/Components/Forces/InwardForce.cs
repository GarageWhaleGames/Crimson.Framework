using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Components.Forces
{
	public struct InwardForce : IComponentData
	{
		public float Force;
		public int ForceMode;
		public Vector3 SourcePosition;
	}
}