using System;
using Crimson.Core.Components;
using Unity.Mathematics;
using UnityEngine;

namespace Crimson.Core.Utils
{
    public static class MathUtils
    {
        public static float Clamp(float number, float min, float max)
        {
            return number > max ? max : number < min ? min : number;
        }

        public static float ApplyDynamics(ref ActorMovementData movement, float time)
        {
            float multiplier = 1f;
            var move = movement.Input;

            //All of this is for smooth movement starts and ends, according to Curves in Component
            if (!move.Equals(float3.zero))
            {
                movement.CurveOutStartTime = 0f;

                if (Math.Abs(movement.CurveInStartTime) < 0.001f)
                {
                    movement.CurveInStartTime = time;
                }

                var timePassed = time - movement.CurveInStartTime;

                if (timePassed < movement.Dynamics.timeScaleIn)
                {
                    movement.InRatio = timePassed / movement.Dynamics.timeScaleIn;
                    multiplier =
                        movement.Dynamics.curveIn.Evaluate(movement.InRatio);
                    if (movement.OutRatio < 1f)
                    {
                        multiplier =
                            Clamp(multiplier +
                                  movement.Dynamics.curveOut.Evaluate(movement.OutRatio), 0f, 1f);
                    }
                }
                else
                {
                    movement.InRatio = movement.OutRatio = 1f;
                    multiplier = 1f;
                }

                movement.MovementCache = move;
            }
            else
            {
                movement.CurveInStartTime = 0f;

                if (Math.Abs(movement.CurveOutStartTime) < 0.001f)
                {
                    movement.CurveOutStartTime = time;
                }

                var timePassed = time - movement.CurveOutStartTime;

                if (timePassed < movement.Dynamics.timeScaleOut)
                {
                    movement.OutRatio = timePassed / movement.Dynamics.timeScaleOut;
                    multiplier =
                        movement.Dynamics.curveOut.Evaluate(movement.OutRatio);
                    if (movement.InRatio < 1f)
                    {
                        multiplier =
                            Clamp(multiplier - movement.Dynamics.curveIn.Evaluate(movement.InRatio),
                                0f, 1f);
                    }
                }
                else
                {
                    movement.InRatio = movement.OutRatio = 1f;
                    multiplier = 0f;
                    movement.MovementCache = move;
                }
            }

			if (movement.Dynamics.workTime > 0 && time - movement.CurveInStartTime > movement.Dynamics.workTime)
				multiplier = 0;

			return multiplier;
        }

        public static float2 RotateVector(float2 v, float angle)
        {
            var radian = math.radians(angle);
            math.sincos(radian, out var sin, out var cos);
            var x = v.x * cos - v.y * sin;
            var y = v.x * sin + v.y * cos;
            return new float2(x, y);
        }

        public static float3 ClampMagnitude(float3 vector, float maxLength)
        {
            var sqrMagnitude = math.lengthsq(vector);
            if (sqrMagnitude <= maxLength * (double) maxLength)
                return vector;
            var num1 = (float) Math.Sqrt(sqrMagnitude);
            var num2 = vector.x / num1;
            var num3 = vector.y / num1;
            var num4 = vector.z / num1;
            return new float3(num2 * maxLength, num3 * maxLength, num4 * maxLength);
        }

        public static float3 Reflect(this float3 inDirection, float3 inNormal)
        {
            float num = -2f * Dot(inNormal, inDirection);
            return new float3(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y,
                num * inNormal.z + inDirection.z);
        }

        public static float Dot(float3 vectorValue, float3 float3Value)
        {
            return (float) ((double) vectorValue.x * float3Value.x + (double) vectorValue.y * float3Value.y +
                            (double) vectorValue.z * (double) float3Value.z);
        }

        public static bool InRange(this int number, int min, int max)
        {
            return number >= min && number <= max;
        }
        
        public static float AngleTo(Vector2 pos, Vector2 target)
        {
            Vector2 diff = Vector2.zero;

            if (target.x > pos.x)
                diff = target - pos;
            else
                diff = pos - target;

            return Vector2.Angle(Vector2.right, diff);
        }
    }
}