using System;
using Elements.Data;

namespace Elements.Core
{
    [DataModelType]
    public readonly struct Transform : IEquatable<Transform>
    {
        public readonly float3 position;
        public readonly floatQ rotation;
        public readonly float3 scale;

        public Transform(in float3 position, in floatQ rotation, in float3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public bool Equals(Transform other) =>
            position == other.position && rotation == other.rotation && scale == other.scale;

        public static bool operator ==(Transform a, Transform b) => a.Equals(b);
        public static bool operator !=(Transform a, Transform b) => !a.Equals(b);

        public override bool Equals(object obj) => obj is Transform other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(position, rotation, scale);

        public static implicit operator Renderite.Shared.RenderTransform(Transform t) => new Renderite.Shared.RenderTransform(t.position, t.rotation, t.scale);
        public static implicit operator Transform(Renderite.Shared.RenderTransform t) => new Transform(t.position, t.rotation, t.scale);
    }
}
