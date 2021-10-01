using System.Collections.Generic;
using Crimson.Core.Common;
using Crimson.Core.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace Crimson.Core.Utils
{
    [HideMonoScript]
    public class ResetControls : MonoBehaviour, IActorAbility
    {
        public OnScreenControl[] onScreenControls;
        public IActor Actor { get; set; }

        public Text debugTouchCounter;

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            Actor = actor;
            var dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (dstManager.HasComponent<ExecuteInUpdate>(entity))
            {
                var a = dstManager.GetComponentObject<ExecuteInUpdate>(entity);
                a.Abilities.Add(this);
                dstManager.SetComponentData(entity, a);
            }
            else
            {
                dstManager.AddComponentData(entity, new ExecuteInUpdate
                {
                    Enabled = true,
                    Abilities = new List<IActorAbility> {this}
                } );
            }

            if (onScreenControls.Length == 0)
            {
                onScreenControls = this.gameObject.GetComponentsInChildren<OnScreenControl>();
            }
            Debug.Log("[RESET CONTROLS HACK] Controls found: " + onScreenControls.Length);
                
        }

        public void Execute()
        {
            if (debugTouchCounter != null) debugTouchCounter.text = Input.touchCount.ToString();
            if (Input.touchCount != 0 || Application.isEditor) return;
            
            foreach (var control in onScreenControls)
            {
                switch (control)
                {
                    case OnScreenButton button:
                        button.OnPointerUp(new PointerEventData(EventSystem.current));
                        break;
                    case OnScreenStick stick:
                        stick.OnPointerUp(new PointerEventData(EventSystem.current));
                        break;
                }
            }
        }
    }

    public class ExecuteInUpdate : IComponentData
    {
        public bool Enabled;
        public List<IActorAbility> Abilities;
    }
}