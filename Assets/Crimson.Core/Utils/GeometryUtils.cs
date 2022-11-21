using System;
using UnityEngine;

namespace Assets.Crimson.Core.Utils
{
	public static class GeometryUtils
	{
		public static bool IsPointInTriangle(Vector2 triangleA, Vector2 triangleB, Vector2 triangleC, Vector2 point)
		{
			//Реализация - считаются произведения(1, 2, 3 - вершины треугольника, 0 - точка):
			//(x1 - x0) * (y2 - y1) - (x2 - x1) * (y1 - y0)
			//(x2 - x0) * (y3 - y2) - (x3 - x2) * (y2 - y0)
			//(x3 - x0) * (y1 - y3) - (x1 - x3) * (y3 - y0)
			//Если они одинакового знака, то точка внутри треугольника, если что-то из этого -ноль, то точка лежит на стороне, иначе точка вне треугольника.
			var sign1 = Math.Sign((triangleA.x - point.x) * (triangleB.y - triangleA.y) - (triangleB.x - triangleA.x) * (triangleA.y - point.y));
			var sign2 = Math.Sign((triangleB.x - point.x) * (triangleC.y - triangleB.y) - (triangleC.x - triangleB.x) * (triangleB.y - point.y));
			var sign3 = Math.Sign((triangleC.x - point.x) * (triangleA.y - triangleC.y) - (triangleA.x - triangleC.x) * (triangleC.y - point.y));
			return sign1 == sign2 && sign2 == sign3;
		}
	}
}