using Assets.Crimson.Core.Loading.SpawnDataTypes;
using Crimson.Core.Common;
using UnityEngine;

namespace Crimson.Core.Loading.SpawnDataTypes
{
	public struct SpawnItemData
	{
		public bool DeleteExistingComponents;
		public IActor Owner;
		public SpawnParentData Parent;
		public Vector3 Position;
		public GameObject Prefab;
		public Quaternion Rotation;
		public Component[] SampledComponents;
		public IActor Spawner;
		public GameObject SpawnPoint;
	}
}