﻿using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Crimson.Core.Common
{
    public class OnScreenCustomButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [InputControl(layout = "Button")]
        [SerializeField]
        public string buttonControlPath;
        protected override string controlPathInternal
        {
            get => buttonControlPath;
            set => buttonControlPath = value;
        }

        private bool _repeatedInvokingOnHold;

        public void SetupButton(bool repeatedInvokingOnHold)
        {
            _repeatedInvokingOnHold = true;// repeatedInvokingOnHold;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            if (_repeatedInvokingOnHold)
            {
                SendValueToControl(1.0f);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            SendValueToControl(_repeatedInvokingOnHold ? 0.0f : 1.0f);
        }

        public void ForceButtonRelease()
        {
            if (_repeatedInvokingOnHold) return;

            SendValueToControl(0.0f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }

    public struct NotifyButtonActionExecutedData : IComponentData
    {
        public int ButtonIndex;
    }
}