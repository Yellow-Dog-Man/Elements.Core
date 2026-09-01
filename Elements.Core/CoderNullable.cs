using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace Elements.Core
{
    public static class CoderNullable
    {
        public static Delegate GetNullableMethod(Type nullableType, Type delegateType, string name)
        {
            var underlyingType = Nullable.GetUnderlyingType(nullableType);
            var coderType = typeof(CoderNullable<>).MakeGenericType(underlyingType);

            var method = coderType.GetMethod(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            return method.CreateDelegate(delegateType);
        }
    }

    public static class CoderNullable<T>
        where T : struct
    {
        /*static M GetNullableMethod<M>(string name)
        {
            var method = typeof(Coder<T>).GetMethod(name, System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Static);

            var genMethod = method.MakeGenericMethod(Nullable.GetUnderlyingType(typeof(T)));

            return (M)(object)genMethod.CreateDelegate(typeof(M));
        }*/

        public static void Dummy() {  }

        public static bool EqualsNullable(T? a, T? b)
        {
            if (a.HasValue != b.HasValue)
                return false;
            if (!a.HasValue)
                return true;

            return Coder<T>.Equals(a.Value, b.Value);
        }

        public static void EncodeNullable(T? value, BinaryWriter bw)
        {
            bw.Write(value.HasValue);
            if (value.HasValue)
                Coder<T>.Encode(value.Value, bw);
        }

        public static T? DecodeNullable(BinaryReader br)
        {
            var hasValue = br.ReadBoolean();

            if (hasValue)
                return Coder<T>.Decode(br);

            return null;
        }

        public static DataTreeNode SaveNullable(T? value)
        {
            if (value.HasValue)
                return Coder<T>.Save(value.Value);
            else
                return new DataTreeValue(null as IConvertible);
        }

        public static T? LoadNullable(DataTreeNode node)
        {
            var valueNode = node as DataTreeValue;

            if (valueNode?.IsNull ?? false)
                return null;

            return Coder<T>.Load(node);
        }

        public static string EncodeToStringNullable(T? value)
        {
            if (value.HasValue)
                return Coder<T>.EncodeToString(value.Value);

            return null;
        }

        public static T? DecodeFromStringNullable(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            return Coder<T>.DecodeFromString(str);
        }
    }
}
