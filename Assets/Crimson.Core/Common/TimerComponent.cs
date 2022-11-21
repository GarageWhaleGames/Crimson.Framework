using Crimson.Core.Components;
using Crimson.Core.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Crimson.Core.Common
{
	[NetworkSimObject, AddComponentMenu("")]
	public class TimerComponent : MonoBehaviour
	{
		[NetworkSimData]
		public List<TimerAction> TimedActions = new List<TimerAction>();

		[NetworkSimData]
		public IActor actor;

		public void Start()
		{
			actor = this.gameObject.GetComponent<IActor>();
			if (actor == null)
			{
				Debug.LogError("[TIMER COMPONENT] No IActor component found, aborting!");
				return;
			}

			var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (dstManager.Exists(actor.ActorEntity))
			{
				dstManager.AddComponent<TimerData>(actor.ActorEntity);
				dstManager.AddComponentObject(actor.ActorEntity, this);
			}
		}

		public void DestroyWithEntity()
		{
			this.gameObject.DestroyWithEntity(actor.ActorEntity);
		}

#if UNITY_EDITOR

		[ShowInInspector]
		private List<TimerActionEditor> Actions
		{
			get
			{
				return TimedActions.Select(action => new TimerActionEditor
				{ Act = action.Act.Method.Name, Delay = action.Delay }).ToList();
			}
		}

		[Button(ButtonSizes.Medium)]
		private void StopTimers()
		{
			var t = this.gameObject.GetComponent<ITimer>();
			t?.FinishTimer();
		}

		[Button(ButtonSizes.Medium)]
		private void StartTimers()
		{
			var t = this.gameObject.GetComponent<ITimer>();
			t?.StartTimer();
		}

		private void Reset()
		{
			Debug.LogError("[TIMER COMPONENT] Timer Component is not allowed to add manually in the editor!");
			DestroyImmediate(this);
		}

#endif
	}

	public struct TimerAction
	{
		public Action Act;
		public float Delay;
	}

	public struct TimerActionEditor
	{
		public string Act;
		public float Delay;
	}
}