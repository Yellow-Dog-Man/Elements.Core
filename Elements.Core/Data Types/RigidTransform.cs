using System;
using Elements.Data;

namespace Elements.Core
{
    [DataModelType]
    public readonly struct RigidTransform : IEquatable<RigidTransform>
    {
        public readonly float3 position;
        public readonly floatQ rotation;

        public RigidTransform(in float3 position, in floatQ rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public bool Equals(RigidTransform other) => position == other.position && rotation == other.rotation;

        public static bool operator ==(RigidTransform a, RigidTransform b) => a.Equals(b);
        public static bool operator !=(RigidTransform a, RigidTransform b) => !a.Equals(b);
        public override bool Equals(object obj) => obj is RigidTransform other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(position, rotation);
    }
}