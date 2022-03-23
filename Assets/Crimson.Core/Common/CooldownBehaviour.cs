﻿using Crimson.Core.Components;
using Crimson.Core.Utils;
using System;

namespace Crimson.Core.Common
{
    public class CooldownBehaviour : TimerBaseBehaviour, IEnableable
    {
        public bool Enabled { get; set; } = true;

        public void ApplyActionWithCooldown(float cooldownTime, Action action)
        {
            if (!Enabled) return;

            action.Invoke();

            if (Math.Abs(cooldownTime) < 0.1f) return;

            StartTimer();
            Timer.TimedActions.AddAction(FinishTimer, cooldownTime);
        }

        public override void FinishTimer()
        {
            base.FinishTimer();
            Enabled = true;
        }

        public override void StartTimer()
        {
            base.StartTimer();
            Enabled = false;
        }
    }
}