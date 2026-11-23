using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Elements.Core
{
    public static partial class MathX
    {
        public const float FLOAT_EPSILON = 1.17549435E-38f;
        public const double DOUBLE_EPSILON = 2.22507385850720138309023271733E-308;

        public const float APPROXIMATELY_FLOAT_EPSILON = 1e-6f;
        public const double APPROXIMATELY_DOUBLE_EPSILON = APPROXIMATELY_FLOAT_EPSILON;

        // Constants
        public const float QUARTER_PI = PI / 4;
        public const float HALF_PI = PI / 2;
        public const float PI = 3.14159265358979323f;
        public const float TAU = PI * 2;

        public const float INV_QUARTER_PI = 1f / QUARTER_PI;
        public const float INV_HALF_PI = 1f / HALF_PI;
        public const float INV_PI = 1f / PI;
        public const float INV_TAU = 1f / TAU;

        public const float SQRT2 = 1.41421356237f;

        public const float E = 2.718281828459045235360287471352f;

        public const float PHI = 1.6180339887498948482f;

        public const float Deg2Rad = 0.017453292519f;
        public const float Rad2Deg = 57.29577951308f;

        public const float EXP_ROUNDING_OFFSET = 0.08496250072116f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(in floatQ value) => !value.IsNaN && !value.IsInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static floatQ FilterInvalid(in floatQ value) => FilterInvalid(value, floatQ.Identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static floatQ FilterInvalid(in floatQ value, in floatQ fallback)
        {
            if (IsValid(value))
                return value;

            return fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(in doubleQ value) => !value.IsNaN && !value.IsInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static doubleQ FilterInvalid(in doubleQ value) => FilterInvalid(value, doubleQ.Identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static doubleQ FilterInvalid(in doubleQ value, in doubleQ fallback)
        {
            if (IsValid(value))
                return value;

            return fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(System.Numerics.Vector3 value)
        {
            if (float.IsNaN(value.X) || float.IsNaN(value.Y) || float.IsNaN(value.Z) ||
                float.IsInfinity(value.X) || float.IsInfinity(value.Y) || float.IsInfinity(value.Z))
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(System.Numerics.Quaternion value)
        {
            if (float.IsNaN(value.X) || float.IsNaN(value.Y) || float.IsNaN(value.Z) || float.IsNaN(value.W) ||
                float.IsInfinity(value.X) || float.IsInfinity(value.Y) || float.IsInfinity(value.Z) || float.IsInfinity(value.W))
                return false;

            return true;
        }

        public static T Mask<T>(this T value, bool mask, T masked = default) where T : unmanaged => mask ? value : masked;

        public static double BitsToBytes(double bits) => bits / 8;
        public static double BytesToBits(double bytes) => bytes * 8;
        public static int BitsToBytes(int bits) => bits >> 3;
        public static int BytesToBits(int bytes) => bytes << 3;

        public static long BitsToBytes(long bits) => bits >> 3;
        public static long BytesToBits(long bytes) => bytes << 3;

        public static bool IsPowerOfTwo(int value) => (value != 0) && ((value & (value - 1)) == 0);
        public static bool IsPowerOfTwo(ulong value) => (value != 0) && ((value & (value - 1)) == 0);

        public static int NearestPowerOfTwo(int value)
        {
            var exp = Log(value, 2f);
            exp = Round(exp + EXP_ROUNDING_OFFSET);

            return RoundToInt(Pow(2, exp));
        }

        public static int2 NearestPowerOfTwo(in int2 value)
        {
            var exp = Log(value, 2f);
            exp = Round(exp + EXP_ROUNDING_OFFSET);

            return RoundToInt(Pow(2, exp));
        }

        public static int3 NearestPowerOfTwo(in int3 value)
        {
            var exp = Log(value, 2f);
            exp = Round(exp + EXP_ROUNDING_OFFSET);

            return RoundToInt(Pow(2, exp));
        }

        public static int4 NearestPowerOfTwo(in int4 value)
        {
            var exp = Log(value, 2f);
            exp = Round(exp + EXP_ROUNDING_OFFSET);

            return RoundToInt(Pow(2, exp));
        }

        // based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
        public static int CeilToPowerOfTwo(int value)
        {
            value--;

            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;

            value++;

            return value;
        }

        public static TimeSpan Abs(TimeSpan timespan) => (timespan < TimeSpan.Zero) ? -timespan : timespan;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b, float epsilon = APPROXIMATELY_FLOAT_EPSILON)
        {
            return Abs(a - b) < Max(1e-6f * Max(Abs(a), Abs(b)), epsilon);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(double a, double b, double epsilon = APPROXIMATELY_DOUBLE_EPSILON)
        {
            return Abs(a - b) < Max(1e-12 * Max(Abs(a), Abs(b)), epsilon);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(in floatQ a, in floatQ b, float epsilon = APPROXIMATELY_FLOAT_EPSILON) => Dot(a, b) >= (1 - epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(in doubleQ a, in doubleQ b, double epsilon = APPROXIMATELY_DOUBLE_EPSILON) => Dot(a, b) >= (1 - epsilon);

        public static float LimitDecimalPlaces(float value, int decimalPlaces)
        {
            if (decimalPlaces > 50)
                return value;

            float mul = MathX.Pow(10, decimalPlaces);

            value *= mul;
            value = MathX.Round(value);
            value /= mul;

            return value;
        }

        public static double LimitDecimalPlaces(double value, int decimalPlaces)
        {
            if (decimalPlaces > 50)
                return value;

            float mul = MathX.Pow(10, decimalPlaces);

            value *= mul;
            value = MathX.Round(value);
            value /= mul;

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan2(in float2 v)
        {
            return Atan2(v.y, v.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Atan2(in double2 v)
        {
            return Atan2(v.y, v.x);
        }

        // Integer operations

        public static ulong MaxValueForBits(int bits)
        {
            if (bits == 0)
                return 0;
            if (bits <= 0)
                throw new System.Exception("Number of bits cannot be negative");

            return (ulong.MaxValue >> (sizeof(ulong) * 8 - bits));
        }

        // returns minimum number of bits needed to represent this number
        public static int NecessaryBits(long number)
        {
            return NecessaryBits((ulong)number);
        }

        public static int NecessaryBits(ulong number)
        {
            int bits = 0;

            while (number != 0)
            {
                number >>= 1;
                bits++;
            }

            return bits;
        }

        public static ulong BitRangeMask(int bitCount, int bitOffset = 0)
        {
            return ((~(0UL)) >> (sizeof(ulong) * 8 - bitCount)) << bitOffset;
        }

        public static double DecimalPoints(double value, int decimalPoints)
        {
            var divisor = Pow(10, decimalPoints);

            value *= divisor;
            value = RoundToLong(value);
            value /= divisor;

            return value;
        }

        // Floating point and vector operations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pack(this float n)
        {
            return n * 0.5f + 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Unpack(this float n)
        {
            return n * 2f - 1f;
        }

        public static float2 Rotate90CW(in float2 v) => new float2(-v.y, v.x);
        public static float2 Rotate90CCW(in float2 v) => new float2(v.y, -v.x);

        public static float2 Rotate(in float2 v, float angle) => RotateRad(v, angle * Deg2Rad);

        public static float2 RotateRad(in float2 v, float angle)
        {
            // compute rotation matrix
            var rot00 = Cos(angle);
            var rot01 = Sin(angle);
            var rot10 = -rot01;
            var rot11 = rot00;

            // multiply
            float _x = 0f, _y = 0f;

            _x += v.x * rot00;
            _x += v.y * rot10;

            _y += v.x * rot01;
            _y += v.y * rot11;

            return new float2(_x, _y);
        }

        public enum ArrayWrap { Clamp, Repeat };

        // Angle

        public static float DeltaAngle(float from, float to = 0f)
        {
            var delta = (to - from) % 360;

            if (delta > 180)
                delta -= 360;
            if (delta < -180)
                delta += 360f;

            return delta;
        }

        public static double DeltaAngle(double from, double to)
        {
            var delta = (to - from) % 360;

            if (delta > 180)
                delta -= 360;
            if (delta < -180)
                delta += 360f;

            return delta;
        }

        // Handy rounding
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(float val) => (int)(val + (val < 0 ? -0.5f : 0.5f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToInt(float val) => (int)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilToInt(float val) => (int)Ceil(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RoundToLong(float val) => (long)(val + (val < 0 ? -0.5f : 0.5f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FloorToLong(float val) => (long)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CeilToLong(float val) => (long)Ceil(val);

        // double

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(double val) => (int)(val + (val < 0 ? -0.5 : 0.5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToInt(double val) => (int)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilToInt(double val) => (int)Ceil(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RoundToLong(double val) => (long)(val + (val < 0 ? -0.5 : 0.5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FloorToLong(double val) => (long)val;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CeilToLong(double val) => (long)Ceil(val);

        // unsigned

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundToUInt(float val) => (uint)(val + (val < 0 ? -0.5f : 0.5f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FloorToUInt(float val) => (uint)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CeilToUInt(float val) => (uint)Ceil(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RoundToULong(float val) => (ulong)(val + (val < 0 ? -0.5f : 0.5f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FloorToULong(float val) => (ulong)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CeilToULong(float val) => (ulong)Ceil(val);

        // double

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundToUInt(double val) => (uint)(val + (val < 0 ? -0.5 : 0.5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FloorToUInt(double val) => (uint)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CeilToUInt(double val) => (uint)Ceil(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RoundToULong(double val) => (ulong)(val + (val < 0 ? -0.5 : 0.5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FloorToULong(double val) => (ulong)Floor(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CeilToULong(double val) => (ulong)Ceil(val);

        // Regular lerping
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static half LerpUnclamped(half a, half b, float lerp) => half.Lerp(a, b, (half)lerp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpUnclamped(float a, float b, float lerp) => a + (b - a) * lerp;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpUnclamped(double a, double b, double lerp) => a + (b - a) * lerp;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpUnclamped(double a, double b, float lerp) => a + (b - a) * lerp;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal LerpUnclamped(decimal a, decimal b, decimal lerp) => a + (b - a) * lerp;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal LerpUnclamped(decimal a, decimal b, float lerp) => LerpUnclamped(a, b, (decimal)lerp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LerpUnclamped(ulong a, ulong b, double lerp) => MathX.RoundToULong((double)a + ((double)b - (double)a) * lerp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static half Lerp(half a, half b, float lerp)
        {
            if (lerp <= 0)
                return a;
            if (lerp >= 1)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float lerp)
        {
            if (lerp <= 0)
                return a;
            if (lerp >= 1)
                return b;

            return a + (b - a) * lerp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float a, float b, float value)
        {
            value -= a;
            value /= (b - a);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double a, double b, double lerp)
        {
            if (lerp <= 0)
                return a;
            if (lerp >= 1)
                return b;

            return a + (b - a) * lerp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double a, double b, float lerp)
        {
            if (lerp <= 0)
                return a;
            if (lerp >= 1)
                return b;

            return a + (b - a) * lerp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double InverseLerp(double a, double b, double value)
        {
            value -= a;
            value /= (b - a);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal InverseLerp(decimal a, decimal b, decimal value)
        {
            value -= a;
            value /= (b - a);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Lerp(decimal a, decimal b, decimal lerp)
        {
            if (lerp <= 0)
                return a;
            if (lerp >= 1)
                return b;

            return a + (b - a) * lerp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Lerp(decimal a, decimal b, float lerp)
        {
            if (lerp <= 0)
                return a;
            if (lerp >= 1)
                return b;

            return a + (b - a) * (decimal)lerp;
        }

        // Projecting point on a line

        // For completeness
        public static float LineInverseLerp(float a, float b, float value) => InverseLerp(a, b, value);

        public static float LineInverseLerp(in float2 a, in float2 b, in float2 value)
        {
            var line = b - a;
            var point = value - a;

            var invMag = 1f / line.Magnitude;

            return MathX.Dot(line * invMag, point * invMag);
        }

        public static float LineInverseLerp(in float3 a, in float3 b, in float3 value)
        {
            var line = b - a;
            var point = value - a;

            var invMag = 1f / line.Magnitude;

            return MathX.Dot(line * invMag, point * invMag);
        }

        public static float LineInverseLerp(in float4 a, in float4 b, in float4 value)
        {
            var line = b - a;
            var point = value - a;

            var invMag = 1f / line.Magnitude;

            return MathX.Dot(line * invMag, point * invMag);
        }

        // Based on: https://math.stackexchange.com/questions/2346982/slerp-inverse-given-3-quaternions-find-t
        public static float RotationInverseLerp(in floatQ a, in floatQ b, in floatQ value)
        {
            var aInv = a.Inverted;

            var valDelta = aInv * value;
            var bDelta = aInv * b;

            // If the quaternion is flipped, sometimes Log produces huge negative/positive values
            // pick whichever one is smaller, to produce lerp value that's closest to either of the two points
            var valLog0 = Log(valDelta);
            var valLog1 = Log(-valDelta);

            var bLog0 = Log(bDelta);
            var bLog1 = Log(-bDelta);

            float3 valLog, bLog;

            if (MathX.Abs(valLog0.x + valLog0.y + valLog0.z) < MathX.Abs(valLog1.x + valLog1.y + valLog1.z))
                valLog = valLog0;
            else
                valLog = valLog1;

            if (MathX.Abs(bLog0.x + bLog0.y + bLog0.z) < MathX.Abs(bLog1.x + bLog1.y + bLog1.z))
                bLog = bLog0;
            else
                bLog = bLog1;

            float lerp = 0;
            int count = 0;

            if (bLog.x != 0)
            {
                lerp += valLog.x / bLog.x;
                count++;
            }

            if (bLog.y != 0)
            {
                lerp += valLog.y / bLog.y;
                count++;
            }

            if (bLog.z != 0)
            {
                lerp += valLog.z / bLog.z;
                count++;
            }

            //return MathX.AvgComponent(lerp);
            return lerp / count;
        }

        public static float3 Log(in floatQ q) => Acos(q.w) * q.xyz.Normalized;

        // For completeness
        public static double LineInverseLerp(double a, double b, double value) => InverseLerp(a, b, value);

        public static double LineInverseLerp(in double2 a, in double2 b, in double2 value)
        {
            var line = b - a;
            var point = value - a;

            var invMag = 1f / line.Magnitude;

            return MathX.Dot(line * invMag, point * invMag);
        }

        public static double LineInverseLerp(in double3 a, in double3 b, in double3 value)
        {
            var line = b - a;
            var point = value - a;

            var invMag = 1f / line.Magnitude;

            return MathX.Dot(line * invMag, point * invMag);
        }

        public static double LineInverseLerp(in double4 a, in double4 b, in double4 value)
        {
            var line = b - a;
            var point = value - a;

            var invMag = 1f / line.Magnitude;

            return MathX.Dot(line * invMag, point * invMag);
        }

        // Integer lerping

        public static int Lerp(int a, int b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static long Lerp(long a, long b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static uint Lerp(uint a, uint b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static ulong Lerp(ulong a, ulong b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static int LerpUnclamped(int a, int b, float lerp)
        {
            if (a < b)
                return RoundToInt(a + (b - a) * (double)lerp);
            else
                return RoundToInt(b + (a - b) * (double)(1 - lerp));
        }

        public static long LerpUnclamped(long a, long b, float lerp)
        {
            if (a < b)
                return RoundToLong(a + (b - a) * (double)lerp);
            else
                return RoundToLong(b + (a - b) * (double)(1 - lerp));
        }

        public static uint LerpUnclamped(uint a, uint b, float lerp)
        {
            if (a < b)
                return RoundToUInt(a + (b - a) * (double)lerp);
            else
                return RoundToUInt(b + (a - b) * (double)(1 - lerp));
        }

        public static ulong LerpUnclamped(ulong a, ulong b, float lerp)
        {
            if (a < b)
                return RoundToULong(a + (b - a) * (double)lerp);
            else
                return RoundToULong(b + (a - b) * (double)(1 - lerp));
        }

        public static char Lerp(char a, char b, float lerp) => (char)Lerp((uint)a, (uint)b, lerp);
        public static byte Lerp(byte a, byte b, float lerp) => (byte)Lerp((uint)a, (uint)b, lerp);
        public static sbyte Lerp(sbyte a, sbyte b, float lerp) => (sbyte)Lerp((int)a, (int)b, lerp);
        public static ushort Lerp(ushort a, ushort b, float lerp) => (ushort)Lerp((uint)a, (uint)b, lerp);
        public static short Lerp(short a, short b, float lerp) => (short)Lerp((int)a, (int)b, lerp);

        public static char LerpUnclamped(char a, char b, float lerp) => (char)LerpUnclamped((uint)a, (uint)b, lerp);
        public static byte LerpUnclamped(byte a, byte b, float lerp) => (byte)LerpUnclamped((uint)a, (uint)b, lerp);
        public static sbyte LerpUnclamped(sbyte a, sbyte b, float lerp) => (sbyte)LerpUnclamped((int)a, (int)b, lerp);
        public static ushort LerpUnclamped(ushort a, ushort b, float lerp) => (ushort)LerpUnclamped((uint)a, (uint)b, lerp);
        public static short LerpUnclamped(short a, short b, float lerp) => (short)LerpUnclamped((int)a, (int)b, lerp);

        public static float InverseLerp(int a, int b, int value)
        {
            value -= a;
            return value / (float)(b - a);
        }

        public static float InverseLerp(long a, long b, long value)
        {
            value -= a;
            return value / (float)(b - a);
        }

        public static float InverseLerp(ulong a, ulong b, ulong value) => (float)InverseLerp((double)a, (double)b, (double)value);
        public static float InverseLerp(uint a, uint b, uint value) => (float)InverseLerp((double)a, (double)b, (double)value);

        public static float InverseLerp(DateTime a, DateTime b, DateTime value)
        {
            var rangeTicks = (b - a).Ticks;
            var valueTicks = (value - a).Ticks;
            return valueTicks / (float)rangeTicks;
        }

        public static double InverseLerpDouble(int a, int b, int value)
        {
            value -= a;
            return value / (double)(b - a);
        }

        public static double InverseLerpDouble(long a, long b, long value)
        {
            value -= a;
            return value / (double)(b - a);
        }

        public static double InverseLerpDouble(DateTime a, DateTime b, DateTime value)
        {
            var rangeTicks = (b - a).Ticks;
            var valueTicks = (value - a).Ticks;
            return valueTicks / (double)rangeTicks;
        }

        public static DateTime Lerp(DateTime a, DateTime b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static TimeSpan Lerp(TimeSpan a, TimeSpan b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static DateTime LerpUnclamped(DateTime a, DateTime b, float lerp) => a.AddTicks((long)((b - a).Ticks * lerp));
        public static TimeSpan LerpUnclamped(TimeSpan a, TimeSpan b, float lerp) => new TimeSpan(a.Ticks + (long)((b - a).Ticks * lerp));


        public static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
        public static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
        public static DateTime Clamp(DateTime value, DateTime min, DateTime max) => Max(Min(value, max), min);

        // Cubic lerping
        public static float CubicLerp(float from, float fromTangent, float to, float toTangent, float lerp)
        {
            var l3 = Pow(lerp, 3);
            var l2 = Pow(lerp, 2);
            var l32 = l3 * 2;
            var l22 = l2 * 2;
            var l23 = l2 * 3;
            return ((l32 - l23 + 1) * from) + ((l3 - l22 + lerp) * fromTangent) + ((-l32 + l23) * to) + ((l3 - l2) * toTangent);
        }

        public static double CubicLerp(double from, double fromTangent, double to, double toTangent, double lerp)
        {
            var l3 = Pow(lerp, 3);
            var l2 = Pow(lerp, 2);
            var l32 = l3 * 2;
            var l22 = l2 * 2;
            var l23 = l2 * 3;
            return ((l32 - l23 + 1) * from) + ((l3 - l22 + lerp) * fromTangent) + ((-l32 + l23) * to) + ((l3 - l2) * toTangent);
        }

        #region SNAPPING

        public static float Snap(float value, float positionIncrement)
        {
            var first = value - (value % positionIncrement);
            var second = value + positionIncrement;

            if ((value - first) < (second - value))
                return first;
            else
                return second;
        }

        public static double Snap(double value, double positionIncrement)
        {
            var first = value - (value % positionIncrement);
            var second = value + positionIncrement;

            if ((value - first) < (second - value))
                return first;
            else
                return second;
        }

        public static floatQ Snap(in floatQ orientation, float angleIncrement) => floatQ.Euler(Snap(orientation.EulerAngles, angleIncrement));

        //public static floatQ Snap(floatQ orientation, float angleIncrement) => snap

        #endregion

        #region LERPING COLLECTIONS

        public static List<float> Lerp(List<float> a, List<float> b, float lerp)
        {
            if (a.Count != b.Count)
                throw new System.Exception("Lists must have same length.");

            var n = new List<float>();
            n.Capacity = a.Count;

            for (int i = 0; i < a.Count; i++)
                n.Add(MathX.Lerp(a[i], b[i], lerp));

            return n;
        }

        public static List<float2> Lerp(List<float2> a, List<float2> b, float lerp)
        {
            if (a.Count != b.Count)
                throw new System.Exception("Lists must have same length.");

            var n = new List<float2>();
            n.Capacity = a.Count;

            for (int i = 0; i < a.Count; i++)
                n.Add(MathX.Lerp(a[i], b[i], lerp));

            return n;
        }

        public static List<float3> Lerp(List<float3> a, List<float3> b, float lerp)
        {
            if (a.Count != b.Count)
                throw new System.Exception("Lists must have same length.");

            var n = new List<float3>();
            n.Capacity = a.Count;

            for (int i = 0; i < a.Count; i++)
                n.Add(MathX.Lerp(a[i], b[i], lerp));

            return n;
        }

        public static List<float4> Lerp(List<float4> a, List<float4> b, float lerp)
        {
            if (a.Count != b.Count)
                throw new System.Exception("Lists must have same length.");

            var n = new List<float4>();
            n.Capacity = a.Count;

            for (int i = 0; i < a.Count; i++)
                n.Add(MathX.Lerp(a[i], b[i], lerp));

            return n;
        }

        #endregion

        // Multilerp

        public static float MultiLerp(float lerp, params float[] values)
        {
            lerp = Clamp01(lerp) * (values.Length - 1);

            // find the two neighboring elements
            int index = Min((int)lerp, values.Length - 2);
            float ratio = lerp - index;

            return LerpUnclamped(values[index], values[index + 1], ratio);
        }

        public static float MultiInverseLerp(float value, params float[] values)
        {
            int index = 0;

            for (int i = 0; i < values.Length - 1; i++)
                if (values[i + 1] >= value)
                {
                    index = i;
                    break;
                }

            var invLerp = InverseLerp(values[index], values[index + 1], value);

            float ratio = 1f / values.Length;

            return index * ratio + invLerp * ratio;
        }

        public static float DampingFactor(float damping, float deltaTime) => Pow(1 - damping, deltaTime);

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Max(0.0001f, smoothTime);

            float omega = smoothTime * 0.5f;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float change = current - target;
            float max = maxSpeed * smoothTime;
            float originalTarget = target;

            change = Clamp(change, -max, max);

            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;

            currentVelocity = (currentVelocity - omega * temp) * exp;

            float result = target + (change + temp) * exp;

            if (originalTarget - current > 0 == result > originalTarget)
            {
                result = target;
                currentVelocity = (result - originalTarget) / deltaTime;
            }

            return result;
        }

        public static float ConstantLerp(float current, float target, float delta)
        {
            if (float.IsNaN(current) || float.IsNaN(target) || float.IsNaN(delta))
                return float.NaN;

            var dir = target - current;

            if (Abs(dir) < delta)
                return target;

            return current + Sign(dir) * delta;
        }

        public static float2 ConstantLerp(in float2 current, in float2 target, float delta)
        {
            if (current.IsNaN || target.IsNaN || float.IsNaN(delta))
                return float2.NaN;

            var dir = target - current;
            dir = dir.GetNormalized(out float distance);

            if (distance < delta)
                return target;

            return current + dir * delta;
        }

        public static float3 ConstantLerp(in float3 current, in float3 target, float delta)
        {
            if (current.IsNaN || target.IsNaN || float.IsNaN(delta))
                return float3.NaN;

            var dir = target - current;
            dir = dir.GetNormalized(out float distance);

            if (distance < delta)
                return target;

            return current + dir * delta;
        }

        public static float4 ConstantLerp(in float4 current, in float4 target, float delta)
        {
            if (current.IsNaN || target.IsNaN || float.IsNaN(delta))
                return float4.NaN;

            var dir = target - current;
            dir = dir.GetNormalized(out float distance);

            if (distance < delta)
                return target;

            return current + dir * delta;
        }

        public static floatQ ConstantSlerp(in floatQ current, in floatQ target, float delta)
        {
            delta *= MathX.Deg2Rad;
            var angle = MathX.AngleRad(current, target);

            if (MathX.Approximately(angle, 0))
                return current;

            var ratio = delta / angle;

            return MathX.Slerp(current, target, ratio);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(in color value) => !value.IsNaN && !value.IsInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(in colorX value) => !value.IsNaN && !value.IsInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color FilterInvalid(in color value, in color fallback)
        {
            if (IsValid(value))
                return value;

            return fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static colorX FilterInvalid(in colorX value, in colorX fallback)
        {
            if (IsValid(value))
                return value;

            return fallback;
        }

        public static color ConstantLerp(in color current, in color target, float delta) =>
            new color(
                ConstantLerp(current.r, target.r, delta),
                ConstantLerp(current.g, target.g, delta),
                ConstantLerp(current.b, target.b, delta),
                ConstantLerp(current.a, target.a, delta)
                );

        public static double ConstantLerp(double current, double target, double delta)
        {
            if (double.IsNaN(current) || double.IsNaN(target) || double.IsNaN(delta))
                return double.NaN;

            var dir = target - current;

            if (MathX.Abs(dir) < delta)
                return target;

            return current + Sign(dir) * delta;
        }

        public static double2 ConstantLerp(in double2 current, in double2 target, double delta)
        {
            if (current.IsNaN || target.IsNaN || double.IsNaN(delta))
                return double2.NaN;

            var dir = target - current;
            dir = dir.GetNormalized(out double distance);

            if (distance < delta)
                return target;

            return current + dir * delta;
        }

        public static double3 ConstantLerp(in double3 current, in double3 target, double delta)
        {
            if (current.IsNaN || target.IsNaN || double.IsNaN(delta))
                return double3.NaN;

            var dir = target - current;
            dir = dir.GetNormalized(out double distance);

            if (distance < delta)
                return target;

            return current + dir * delta;
        }

        public static double4 ConstantLerp(in double4 current, in double4 target, double delta)
        {
            if (current.IsNaN || target.IsNaN || double.IsNaN(delta))
                return double4.NaN;

            var dir = target - current;
            dir = dir.GetNormalized(out double distance);

            if (distance < delta)
                return target;

            return current + dir * delta;
        }

        public static doubleQ ConstantSlerp(in doubleQ current, in doubleQ target, float delta)
        {
            return MathX.LimitSwing(target, current * float3.Forward, delta);
        }

        public static color SmoothLerp(in color current, in color target, ref color intermediate, float delta)
        {
            delta *= 2;

            delta = Clamp01(delta);

            intermediate = Lerp(intermediate, target, delta);
            return Lerp(current, intermediate, delta);
        }

        public static colorX SmoothLerp(in colorX current, in colorX target, ref colorX intermediate, float delta)
        {
            delta *= 2;

            delta = Clamp01(delta);

            intermediate = Lerp(intermediate, target, delta);
            return Lerp(current, intermediate, delta);
        }

        public static float3 SmoothSlerp(in float3 current, in float3 target, ref float3 intermediate, float delta)
        {
            // multiplying by two compensates the intermediate
            delta *= 2;

            delta = Clamp01(delta);

            intermediate = Slerp(intermediate, target, delta);

            return Slerp(current, intermediate, delta);
        }

        public static floatQ SmoothSlerp(in floatQ current, in floatQ target, ref floatQ intermediate, float delta)
        {
            // multiplying by two compensates the intermediate
            delta *= 2;

            delta = Clamp01(delta);

            intermediate = Slerp(intermediate, target, delta).FastNormalized;
            return Slerp(current, intermediate, delta).FastNormalized;
        }

        public static doubleQ SmoothSlerp(in doubleQ current, in doubleQ target, ref doubleQ intermediate, double delta)
        {
            // multiplying by two compensates the intermediate
            delta *= 2;

            delta = Clamp01(delta);

            intermediate = Slerp(intermediate, target, delta).FastNormalized;
            return Slerp(current, intermediate, delta).FastNormalized;
        }

        public static float MinSpherePointDistance(float radius, float fieldOfView)
        {
            var a = fieldOfView * 0.5f;

            return radius / Sin(a * Deg2Rad);
        }

        // computing sequences
        public struct LinePoint
        {
            public float3 position;
            public floatQ rotation;
            public float distanceFromStart;
        }

        public static void ComputeLinePoints(float3[] positions, ref LinePoint[] points,
            float3? up = null, bool computeRotations = true, bool computeDistance = true)
        {
            if (points == null || points.Length != positions.Length)
                points = new LinePoint[positions.Length];

            float distAcc = 0f;

            for (int i = 0; i < positions.Length; i++)
            {
                float3 current = positions[i];
                float3 prev = current;
                float3 next = current;

                if (i != positions.Length - 1)
                    next = positions[i + 1];
                if (i != 0)
                    prev = positions[i - 1];

                points[i].position = current;

                // accumulate the distance
                if (computeDistance)
                {
                    if (i > 0)
                        distAcc += MathX.Distance(prev, current);

                    points[i].distanceFromStart = distAcc;
                }

                if (computeRotations)
                {
                    // extrapolate previous point
                    if (i == 0)
                        prev = current - (next - current);
                    if (i == positions.Length - 1)
                        next = current - (prev - current);

                    floatQ r0, r1;

                    if (current - prev == float3.Zero)
                        r0 = r1 = floatQ.Identity;
                    else
                    {
                        // calculate rotations
                        r0 = floatQ.LookRotation((current - prev).Normalized,
                                up ?? float3.Up);
                        r1 = floatQ.LookRotation((next - current).Normalized,
                                up ?? float3.Up);
                    }

                    var r = Slerp(r0, r1, 0.5f);

                    points[i].rotation = r;
                    up = r * float3.Up;
                }
            }
        }

        // processing values
        public static decimal Average(decimal a, decimal b) => (a + b) * 0.5M;
        public static decimal Average(decimal a, decimal b, decimal c) => (a + b + c) * 0.333333333333333333M;

        public static float Average(params float[] vals)
        {
            float sum = 0f;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static float2 Average(params float2[] vals)
        {
            float2 sum = float2.Zero;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static float3 Average(params float3[] vals)
        {
            float3 sum = float3.Zero;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static float4 Average(params float4[] vals)
        {
            float4 sum = float4.Zero;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static double Average(params double[] vals)
        {
            double sum = 0;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static double2 Average(params double2[] vals)
        {
            double2 sum = double2.Zero;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static double3 Average(params double3[] vals)
        {
            double3 sum = double3.Zero;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        public static double4 Average(params double4[] vals)
        {
            double4 sum = double4.Zero;
            foreach (var v in vals)
                sum += v;
            return sum / vals.Length;
        }

        // remapping

        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return ((value - inMin) / (inMax - inMin)) * (outMax - outMin) + outMin;
        }

        public static double Remap(double value, double inMin, double inMax, double outMin, double outMax)
        {
            return ((value - inMin) / (inMax - inMin)) * (outMax - outMin) + outMin;
        }

        public static float Remap11_01(float value)
        {
            return Remap(value, -1, 1, 0, 1);
        }

        public static double Remap11_01(double value)
        {
            return Remap(value, -1, 1, 0, 1);
        }

        // lerping
        public static float Progress01(float current, float deltaTime, float speed = 1f, bool increase = true)
        {
            return Progress01(current, deltaTime * (increase ? 1 : -1) * speed);
        }

        public static float Progress01(float current, float delta)
        {
            return Clamp01(current + delta);
        }

        public static float Progress01(float current, float delta, bool increase)
        {
            return Clamp01(current + (increase ? delta : -delta));
        }

        public static float WrapAroundDistance(float a, float b, float length)
        {
            // make sure a is smaller
            if (a > b)
                (a, b) = (b, a);

            return Min(Abs(a - b), Abs(a - (b - length)));
        }

        public static double WrapAroundDistance(double a, double b, double length)
        {
            // make sure a is smaller
            if (a > b)
                (a, b) = (b, a);

            return Min(Abs(a - b), Abs(a - (b - length)));
        }

        // repeat

        // ensures that it wraps properly when negative
        public static int Repeat(int val, int max)
        {
            // TODO!!! What if max is maxValue?
            max++; // so it can be used as modulo (it would cut the max value otherwise)

            if (max == 0)
                return 0;

            // remember if it was negative
            bool negative = val < 0;

            // set it to zero if it was negative to get a proper start count
            if (negative)
                val++;

            val %= max;

            // if it was negative adjust the negative modulo so it's counting back
            if (negative)
                val = max + val - 1;

            return val;
        }

        public static long Repeat(long val, long max)
        {
            // TODO!!! What if max is maxValue?
            max++; // so it can be used as modulo (it would cut the max value otherwise)

            if (max == 0)
                return 0;

            // remember if it was negative
            bool negative = val < 0;

            // set it to zero if it was negative to get a proper start count
            if (negative)
                val++;

            val %= max;

            // if it was negative adjust the negative modulo so it's counting back
            if (negative)
                val = max + val - 1;

            return val;
        }

        public static uint Repeat(uint val, uint max)
        {
            if (max == 0)
                return 0;

            if (max != uint.MaxValue)
                return val % (max + 1);
            return val;
        }

        public static ulong Repeat(ulong val, ulong max)
        {
            if (max == 0)
                return 0;

            if (max != ulong.MaxValue)
                return val % (max + 1);

            return val;
        }

        public static float Repeat(float val, float length)
        {
            float excess = Floor(val / length) * length;
            return val - excess;
        }

        public static double Repeat(double val, double length)
        {
            double excess = Floor(val / length) * length;
            return val - excess;
        }

        public static decimal Repeat(decimal val, decimal length)
        {
            decimal excess = Floor(val / length) * length;
            return val - excess;
        }

        public static float Repeat01(float val)
        {
            val %= 1.0f;

            if (val < 0)
                return val + 1.0f;
            else
                return val;
        }

        public static double Repeat01(double val)
        {
            val %= 1.0f;

            if (val < 0)
                return val + 1.0f;
            else
                return val;
        }

        public static int PingPong(int val, int length) => Abs((val + length) % (length * 2) - length);
        public static long PingPong(long val, long length) => Abs((val + length) % (length * 2) - length);

        public static uint PingPong(uint val, uint length) => (uint)PingPong((long)val, (long)length);
        public static ulong PingPong(ulong val, ulong length) => (ulong)PingPong((long)val, (long)length);

        public static float PingPong(float val, float length) => Abs((val + length) % (length * 2) - length);
        public static double PingPong(double val, double length) => Abs((val + length) % (length * 2) - length);

        public static int PingPongSafe(int val, int length)
        {
            if (length <= 1)
                return 0;

            return PingPong(val, length);
        }

        public static long PingPongSafe(long val, long length)
        {
            if (length <= 1)
                return 0;

            return PingPong(val, length);
        }

        public static uint PingPongSafe(uint val, uint length)
        {
            if (length <= 1)
                return 0;

            return PingPong(val, length);
        }

        public static ulong PingPongSafe(ulong val, ulong length)
        {
            if (length <= 1)
                return 0;

            return PingPong(val, length);
        }

        public static float PingPongSafe(float val, float length)
        {
            if (length <= 0f)
                return 0;

            return PingPong(val, length);
        }

        public static double PingPongSafe(double val, double length)
        {
            if (length <= 0f)
                return 0;

            return PingPong(val, length);
        }

        public static float PowMagnitude(float val, float pow) => Pow(Abs(val), pow) * Sign(val);
        public static float2 PowMagnitude(float2 val, float pow)
        {
            val = val.GetNormalized(out float magnitude);
            magnitude = MathX.Pow(magnitude, pow);
            return val * magnitude;
        }

        public static float3 PowMagnitude(float3 val, float pow)
        {
            val = val.GetNormalized(out float magnitude);
            magnitude = MathX.Pow(magnitude, pow);
            return val * magnitude;
        }

        public static float4 PowMagnitude(float4 val, float pow)
        {
            val = val.GetNormalized(out float magnitude);
            magnitude = MathX.Pow(magnitude, pow);
            return val * magnitude;
        }

        public static double PowMagnitude(double val, double pow) => Pow(Abs(val), pow) * Sign(val);
        public static double2 PowMagnitude(double2 val, double pow)
        {
            val = val.GetNormalized(out double magnitude);
            magnitude = MathX.Pow(magnitude, pow);
            return val * magnitude;
        }

        public static double3 PowMagnitude(double3 val, double pow)
        {
            val = val.GetNormalized(out double magnitude);
            magnitude = MathX.Pow(magnitude, pow);
            return val * magnitude;
        }

        public static double4 PowMagnitude(double4 val, double pow)
        {
            val = val.GetNormalized(out double magnitude);
            magnitude = MathX.Pow(magnitude, pow);
            return val * magnitude;
        }

        public static double FactorialDouble(int n)
        {
            if (n < 0) // Invalid
                return double.NaN;

            if (n == 0 || n == 1) // Zero or one factorial is just one
                return 1.0;

            double result = n;

            for (int i = n - 1; i > 1; i--)
            {
                result *= i;

                if (result == double.PositiveInfinity)
                    break;
            }

            return result;
        }


        public static float FactorialFloat(int n)
        {
            if (n < 0) // Invalid
                return float.NaN;

            if (n == 0 || n == 1) // Zero or one factorial is just one
                return 1f;

            float result = n;

            for (int i = n - 1; i > 1; i--)
            {
                result *= i;

                if (result == float.PositiveInfinity)
                    break;
            }

            return result;
        }


        public static float BounceLerp01(float val, float bounce)
        {
            val = MathX.Clamp01(val);
            bounce = MathX.Clamp01(bounce);

            float range = MathX.PI * (bounce + 1);

            // remap the value to the range
            val -= 0.5f;
            val *= range;

            // compute the sine
            val = MathX.Sin(val);

            // scale it to the maximum value of the range, so it's 1
            val /= MathX.Sin(range / 2f);

            return val.Pack();  // remap from -1...1 to 0...1
        }

        public static float BounceLerp(float from, float to, float lerp, float bounce)
        {
            float r = BounceLerp01(lerp, bounce);
            return from * (1 - r) + to * r;
        }

        public static int Dir(this bool b) => b ? 1 : -1;
        public static int InvDir(this bool b) => b ? -1 : 1;

        public static bool IsEven(this int i) => (i & 1) == 0;
        public static bool IsOdd(this int i) => (i & 1) != 0;

        #region CLAMPING

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double val, double min, double max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float val)
        {
            if (val < 0f)
                return 0f;
            if (val > 1f)
                return 1f;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp01(double val)
        {
            if (val < 0.0)
                return 0.0;
            if (val > 1.0)
                return 1.0;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int val, int min, int max)
        {
            if (val < min)
                return min;
            if (val >= max)
                return max - 1;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint val, uint min, uint max)
        {
            if (val < min)
                return min;
            if (val >= max)
                return max - 1;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long val, long min, long max)
        {
            if (val < min)
                return min;
            if (val >= max)
                return max - 1;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Clamp(ulong val, ulong min, ulong max)
        {
            if (val < min)
                return min;
            if (val >= max)
                return max - 1;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Clamp(decimal val, decimal min, decimal max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampMagnitude(float v, float maxMagnitude)
        {
            if (v > 0)
                return Min(v, maxMagnitude);
            else
                return Max(v, -maxMagnitude);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampMagnitude(float v, float maxMagnitude, float minMagnitude)
        {
            var mag = Abs(v);

            if (mag > maxMagnitude)
                return maxMagnitude * MathX.Sign(v);

            if (mag < minMagnitude && mag > 0)
                return minMagnitude * MathX.Sign(v);

            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ClampMagnitude(double v, double maxMagnitude, double minMagnitude = 0)
        {
            var mag = Abs(v);

            if (mag > maxMagnitude)
                return maxMagnitude * MathX.Sign(v);

            if (mag < minMagnitude && mag > 0)
                return minMagnitude * MathX.Sign(v);

            return v;
        }

        #endregion

        #region SPECIAL(?)

        public static float Gap01(float value, float gapSize)
        {
            float halfGap = gapSize * 0.5f;
            float gapBegin = 0.5f - halfGap;
            float gapEnd = 0.5f + halfGap;

            if (value <= 0.5f)
                return Lerp(value * 2, 0f, gapBegin);
            else
                return Lerp((value - 0.5f) * 2, gapEnd, 1f);
        }

        public static float ScaleLerp01(float value, float scale)
        {
            value *= scale;
            value -= (scale - 1f) * 0.5f;
            return Clamp01(value);
        }

        #endregion

        public static bool IsBetween(this DateTime v, DateTime from, DateTime to) => v >= from && v <= to;
        public static bool IsBetween(this TimeSpan v, TimeSpan from, TimeSpan to) => v >= from && v <= to;


        public static bool DoIntervalsIntersect(int start0, int end0, int start1, int end1)
        {
            // make sure that 0 is leftmost
            if (start1 < start0)
            {
                (start0, start1) = (start1, start0);
                (end0, end1) = (end1, end0);
            }

            end0 -= start0;
            start1 -= start0;
            end1 -= start0;

            return start1 >= 0 && start1 < end0;
        }

        public static bool DoIntervalsIntersect(float start0, float end0, float start1, float end1)
        {
            // make sure that 0 is leftmost
            if (start1 < start0)
            {
                (start0, start1) = (start1, start0);
                (end0, end1) = (end1, end0);
            }

            end0 -= start0;
            start1 -= start0;
            end1 -= start0;

            return start1 >= 0 && start1 < end0;
        }

        public static bool DoIntervalsIntersect(double start0, double end0, double start1, double end1)
        {
            // make sure that 0 is leftmost
            if (start1 < start0)
            {
                (start0, start1) = (start1, start0);
                (end0, end1) = (end1, end0);
            }

            end0 -= start0;
            start1 -= start0;
            end1 -= start0;

            return start1 >= 0 && start1 < end0;
        }

        // compute a barycentric coordinate of a point in a triangle
        public static float3 BarycentricCoord(in float2 point, in float2 a, in float2 b, in float2 c)
        {
            float2 v0 = b - a;
            float2 v1 = c - a;
            float2 v2 = point - a;

            float d00 = MathX.Dot(v0, v0);
            float d01 = MathX.Dot(v0, v1);
            float d11 = MathX.Dot(v1, v1);
            float d20 = MathX.Dot(v2, v0);
            float d21 = MathX.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1f - v - w;

            return new float3(u, v, w);
        }

        public static float2 BarycentricToCartesian(in float3 coord, in float2 a, in float2 b, in float3 c)
        {
            return new float2(
                coord.x * a.x + coord.y * b.x + coord.z * c.x,
                coord.x * a.y + coord.y * b.y + coord.z * c.y
                );
        }

        public static float Frac(float value) => value - Truncate(value);
        public static double Frac(double value) => value - Truncate(value);
        public static decimal Frac(decimal value) => value - Truncate(value);

        #region NOISE FUNCTIONS

        public static float SimplexNoise(float pos)
        {
            return global::SimplexNoise.Noise.Generate(pos);
        }

        public static float SimplexNoise2D(in float2 pos)
        {
            return global::SimplexNoise.Noise.Generate(pos.x, pos.y);
        }

        public static float SimplexNoise3D(in float3 pos)
        {
            return global::SimplexNoise.Noise.Generate(pos.x, pos.y, pos.z);
        }

        public static float SimplexNoise4D(in float4 pos)
        {
            return global::SimplexNoise.Noise.Generate(pos.x, pos.y, pos.z, pos.w);
        }

        public static float2 SimplexNoise(in float2 pos)
        {
            return new float2(SimplexNoise(pos.x), SimplexNoise(pos.y));
        }

        public static float3 SimplexNoise(in float3 pos)
        {
            return new float3(SimplexNoise(pos.x), SimplexNoise(pos.y), SimplexNoise(pos.z));
        }

        public static float4 SimplexNoise(in float4 pos)
        {
            return new float4(SimplexNoise(pos.x), SimplexNoise(pos.y), SimplexNoise(pos.z), SimplexNoise(pos.w));
        }

        #endregion

        public static float HorizontalFOVFromVerical(float verticalFOV, float aspectRatio)
        {
            verticalFOV *= Deg2Rad;
            var horizontalFOV = 2f * Atan(Tan(verticalFOV * 0.5f) * aspectRatio);
            return horizontalFOV * Rad2Deg;
        }

        public static BoundingBox2D AtlasElementUV(in int2 gridSize, int elementIndex)
        {
            if (gridSize.x <= 0 || gridSize.y <= 0)
                return BoundingBox2D.Empty();

            float2 segmentSize = 1f / gridSize;
            int2 element = new int2(elementIndex % gridSize.x, gridSize.y - elementIndex / gridSize.x - 1);

            return new BoundingBox2D(segmentSize * element, segmentSize * (element + 1));
        }

        public static float Deadzone(float axis, float deadzone)
        {
            var sign = Sign(axis);
            axis = Abs(axis);
            axis -= deadzone;
            axis = Max(0, axis);
            axis /= 1 - deadzone;
            return axis * sign;
        }

        public static float2 RadialDeadzone(in float2 axis, float deadzone)
        {
            var dir = axis.GetNormalized(out float magnitude);

            if (magnitude < deadzone)
                return float2.Zero;

            magnitude -= deadzone;
            magnitude /= 1 - deadzone;

            return dir * magnitude;
        }

        public static float3 RadialDeadzone(in float3 axis, float deadzone)
        {
            var dir = axis.GetNormalized(out float magnitude);

            if (magnitude < deadzone)
                return float3.Zero;

            magnitude -= deadzone;
            magnitude /= 1 - deadzone;

            return dir * magnitude;
        }

        public static float2 ElementWiseDeadzone(float2 axis, float deadzone) =>
            ElementWiseDeadzone(axis, new float2(deadzone, deadzone));
        public static float3 ElementWiseDeadzone(float3 axis, float deadzone) =>
            ElementWiseDeadzone(axis, new float3(deadzone, deadzone, deadzone));

        public static float2 ElementWiseDeadzone(float2 axis, float2 deadzone)
        {
            var sign = Sign(axis);
            axis = Abs(axis);
            axis -= deadzone;
            axis = Max(0, axis);
            axis /= 1 - deadzone;
            return axis * sign;
        }

        public static float3 ElementWiseDeadzone(float3 axis, float3 deadzone)
        {
            var sign = Sign(axis);
            axis = Abs(axis);
            axis -= deadzone;
            axis = Max(0, axis);
            axis /= 1 - deadzone;
            return axis * sign;
        }

        #region GEOMETRY

        public static float3 PerspectiveProjectionRad(in float2 uv, in float2 fov, float distance)
        {
            var offset = (uv - 0.5f) * MathX.Tan(fov);
            return new float3(offset * distance, distance);
        }

        public static float3 PerspectiveProjectionTan(in float2 uv, in float2 tanOfFOV, float distance)
        {
            var offset = (uv - 0.5f) * tanOfFOV;
            return new float3(offset * distance, distance);
        }

        public static float ArcCircumference(float arc, float radius) => arc * Deg2Rad * radius;

        #endregion

        public static void NormalizeSum(float[] values, float targetSum = 1f, bool absoluteValue = false)
        {
            float sum = 0f;

            for (int i = 0; i < values.Length; i++)
            {
                if (absoluteValue)
                    sum += Abs(values[i]);
                else
                    sum += values[i];
            }

            if (sum == 0f)
                return;

            float ratio = targetSum / sum;

            for (int i = 0; i < values.Length; i++)
                values[i] *= ratio;
        }

        #region EXTENSIONS

        public static bool Any(this bool value) => value;

        #endregion

        #region CAMERA PROJECTION

        public static float3 UVToPerspectiveCameraDirection(float2 uv, float aspectRatio, float fieldOfView)
        {
            uv -= 0.5f;
            uv *= new float2(2f, -2f);

            uv *= Tan(fieldOfView * 0.5f * Deg2Rad) * new float2(aspectRatio, 1f);

            return new float3(uv, 1f).Normalized;
        }

        #endregion

        // Based on https://stackoverflow.com/questions/14599487/smallest-multiple

        public static long LeastCommonMultiple(long value0, long value1)
        {
            var a = Abs(value0);
            var b = Abs(value1);

            // perform division first to avoid potential overflow
            a = checked((a / GreatestCommonDivisor(a, b)));
            return checked((a * b));
        }

        public static long GreatestCommonDivisor(long value0, long value1)
        {
            long gcd = 1; // Greatest Common Divisor

            // throw exception if any value=0
            if (value0 == 0 || value1 == 0)
                throw new ArgumentOutOfRangeException();

            // assign absolute values to local vars
            var a = Abs(value0);
            var b = Abs(value1);

            // if numbers are equal return the first
            if (a == b)
                return a;

            // if var "b" is GCD return "b"
            if (a > b && a % b == 0)
                return b;

            // if var "a" is GCD return "a"
            if (b > a && b % a == 0)
                return a;

            // Euclid algorithm to find GCD (a,b):
            // estimated maximum iterations:
            // 5* (number of dec digits in smallest number)
            while (b != 0)
            {
                gcd = b;
                b = a % b;
                a = gcd;
            }

            return gcd;
        }

        #region MEDIAN

        // Based on : https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp

        /// <summary>
        /// Partitions the given list around a pivot element such that all elements on left of pivot are <= pivot
        /// and the ones at thr right are > pivot. This method can be used for sorting, N-order statistics such as
        /// as median finding algorithms.
        /// Pivot is selected ranodmly if random number generator is supplied else its selected as last element in the list.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 171
        /// </summary>
        static int Partition<T>(this IList<T> list, int start, int end, Random rnd = null) where T : IComparable<T>
        {
            if (rnd != null)
                list.Swap(end, rnd.Next(start, end + 1));

            var pivot = list[end];
            var lastLow = start - 1;
            for (var i = start; i < end; i++)
            {
                if (list[i].CompareTo(pivot) <= 0)
                    list.Swap(i, ++lastLow);
            }
            list.Swap(end, ++lastLow);
            return lastLow;
        }

        /// <summary>
        /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
        /// Note: specified list would be mutated in the process.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 216
        /// </summary>
        public static T NthOrderStatistic<T>(this IList<T> list, int n, Random rnd = null) where T : IComparable<T>
        {
            return NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
        }
        static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random rnd) where T : IComparable<T>
        {
            while (true)
            {
                var pivotIndex = list.Partition(start, end, rnd);
                if (pivotIndex == n)
                    return list[pivotIndex];

                if (n < pivotIndex)
                    end = pivotIndex - 1;
                else
                    start = pivotIndex + 1;
            }
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            if (i == j)   //This check is not required but Partition function may make many calls so its for perf reason
                return;

            (list[i], list[j]) = (list[j], list[i]);
        }

        /// <summary>
        /// Note: specified list would be mutated in the process.
        /// </summary>
        public static T ReoderAndComputeMedian<T>(this IList<T> list) where T : IComparable<T> => list.NthOrderStatistic((list.Count - 1) / 2);

        public static double Median<T>(this IEnumerable<T> sequence, Func<T, double> getValue)
        {
            var list = sequence.Select(getValue).ToList();
            var mid = (list.Count - 1) / 2;
            return list.NthOrderStatistic(mid);
        }

        #endregion

        #region REINTERPRETING

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float ReinterpretAsFloat(uint v) => *((float*)&v);

        #endregion

        #region NORMALS

        // https://knarkowicz.wordpress.com/2014/04/16/octahedron-normal-vector-encoding/

        static float2 OctWrap(float2 v) => (1.0f - Abs(v.yx)) * ((v.xy >= 0.0f).Select(float2.One, -float2.One));

        public static float2 EncodeNormalOctahedron(float3 n)
        {
            n /= Abs(n.x) + Abs(n.y) + Abs(n.z);

            var encoded = n.z >= 0.0 ? n.xy : OctWrap(n.xy);
            encoded = encoded * 0.5f + 0.5f;

            return encoded;
        }

        public static float3 DecodeNormalOctahedron(float2 f)
        {
            f = f * 2.0f - 1.0f;

            // https://twitter.com/Stubbesaurus/status/937994790553227264
            var n = new float3(f.x, f.y, 1.0f - Abs(f.x) - Abs(f.y));

            float t = Clamp01(-n.z);
            n = new float3(n.xy + ((n.xy >= 0.0f).Select(float2.One * -t, float2.One * t)), n.z);

            return n.Normalized;
        }

        #endregion
    }
}
