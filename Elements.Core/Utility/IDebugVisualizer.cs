using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    public interface IDebugVisualizer
    {
        void Vector(float3 position, float3 vector, colorX color, float radiusRatio = 1f, float duration = 0f, bool local = false);
        void Line(in float3 point0, in float3 point1, in colorX color, float radius = 0.005f, float duration = 0f, bool local = false);
        void Axes(in float3 position, in floatQ rotation, float length = 0.1f, in colorX? right = null, in colorX? up = null, in colorX? forward = null, float duration = 0, bool local = false);
        void Triangle(float3 point0, float3 point1, float3 point2, in colorX color, float duration = 0f, bool local = false);
        void Sphere(in float3 point, float radius, in colorX color, int subdivisions = 2, float duration = 0f, bool local = false);
        void Plane(in float3 point, in float3 normal, in colorX color, float size = 1f, float duration = 0f, bool local = false);
        void Box(in float3 point, in float3 size, in colorX color, in floatQ orientation, float duration = 0f, bool local = false);
        void Capsule(in float3 point, float height, float radius, in colorX color, in floatQ orientation, float duration = 0f, bool local = false);
        void Cylinder(in float3 point, float height, float radius, in colorX color, in floatQ orientation, float duration = 0f, bool local = false);
        void Cone(in float3 point, float height, float radius, in colorX color, in floatQ orientation, float duration = 0f, bool local = false);
    }
}