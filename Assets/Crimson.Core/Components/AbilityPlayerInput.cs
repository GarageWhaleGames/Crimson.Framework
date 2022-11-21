using Crimson.Core.Common;
using Crimson.Core.Enums;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Components
{
	public struct AIInputData : IComponentData
	{
		public byte foo;
	}

	public struct CompensateCameraRotation : IComponentData
	{
	}

	public struct NetworkInputData : IComponentData
	{
		public byte foo;
	}

	[NetworkSimObject]
	public struct PlayerInputData : IComponentData
	{
		[NetworkSimData] public float CompensateAngle;
		[NetworkSimData] public FixedList512Bytes<float> CustomInput;
		[NetworkSimData] public FixedList512Bytes<float2> CustomSticksInput;
		[NetworkSimData] public float DeclinationAngle;
		[NetworkSimData] public float2 Look;
		public float MinMagnitude;
		[NetworkSimData] public float2 Mouse;
		[NetworkSimData] public float2 Move;
		internal bool InventoryState;

		public override string ToString()
		{
			var results = $"Move:{Move}\n" +
						  $"Mouse:{Mouse}\n" +
						  $"Look:{Look}\n";

			results += $"CustomInput:\n";
			for (var i = 0; i < CustomInput.Length; i++)
			{
				results += $"\t{CustomInput[i]}:{CustomSticksInput[i]}\n";
			}

			return results;
		}
	}

	public struct UserInputData : IComponentData
	{
		public byte foo;
	}

	[HideMonoScript]
	public class AbilityPlayerInput : MonoBehaviour, IActorAbility
	{
		[HideInInspector] public Dictionary<int, List<IActorAbility>> bindingsDict;

		[ShowIfGroup("inputSource", InputSource.UserInput)]
		public bool compensateCameraRotation = true;

		[Space]
		[InfoBox(
			"Bind Abilities calls to Custom Inputs, indexes 0..9 represent keyboard keys of 0..9.\n" +
			"Further bindings are as set in User Input")]
		public List<CustomBinding> customBindings = new List<CustomBinding>();

		[EnumToggleButtons] public InputSource inputSource;

		[ShowIfGroup("inputSource", InputSource.UserInput)]
		public float minMoveInputMagnitude = 0f;

		public IActor Actor { get; set; }

		public void AddComponentData(ref Entity entity, IActor actor)
		{
			bindingsDict = new Dictionary<int, List<IActorAbility>>();

			Actor = actor;

			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			var c = new FixedList512Bytes<float> { Length = Constants.INPUT_BUFFER_CAPACITY };
			var sticksInput = new FixedList512Bytes<float2> { Length = Constants.INPUT_BUFFER_CAPACITY };

			dstManager.AddComponentData(entity, new PlayerInputData
			{
				CustomInput = c,
				MinMagnitude = minMoveInputMagnitude,
				CustomSticksInput = sticksInput
			});

			foreach (var binding in customBindings)
			{
				AddCustomBinding(binding);
			}

			dstManager.AddComponentData(entity, new MoveByInputData());

			switch (inputSource)
			{
				case InputSource.Default:
					break;

				case InputSource.UserInput:
					dstManager.AddComponentData(entity, new UserInputData());
					dstManager.AddComponentData(entity, new NetworkSyncSend());
					if (compensateCameraRotation)
					{
						dstManager.AddComponentData(entity, new CompensateCameraRotation());
					}

					break;

				case InputSource.NetworkInput:
					dstManager.AddComponentData(entity, new NetworkInputData());
					break;

				case InputSource.AIInput:
					dstManager.AddComponentData(entity, new AIInputData());
					dstManager.AddComponentData(entity, new NetworkSyncSend());
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void AddCustomBinding(CustomBinding binding)
		{
			if (!customBindings.Contains(binding)) customBindings.Add(binding);

			if (!bindingsDict.Keys.Contains(binding.index))
			{
				bindingsDict.Add(binding.index, binding.actions.ConvertAll(a => a as IActorAbility));
			}
			else
			{
				bindingsDict[binding.index].AddRange(binding.actions.ConvertAll(a => a as IActorAbility));
			}
		}

		public void Execute()
		{
		}

		public void RemoveCustomBinding(int indexToRemove)
		{
			var bindingToRemove = customBindings.FirstOrDefault(b => b.index == indexToRemove);
			customBindings.Remove(bindingToRemove);

			bindingsDict.Remove(indexToRemove);
		}
	}
}