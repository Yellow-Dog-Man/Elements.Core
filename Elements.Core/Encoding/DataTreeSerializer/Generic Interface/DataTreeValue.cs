using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public partial class DataTreeValue : DataTreeNode
    {
        protected override IEnumerable<DataTreeNode> DirectChildren() => null;

        public IConvertible Value { get; private set; }

        //public DataPrimitiveType Primitive { get; private set; }

        DataTreeValue()
        {
            Value = null;
        }

        public bool IsNull { get { return Value == null; } }

        public bool IsURL
        {
            get
            {
                var str = Value as string;

                if (str == null || str.Length <= 1)
                    return false;

                return str[0] == '@' && str[1] != '@';
            }
        }

        public T Extract<T>()
        {
            if (typeof(T) == typeof(DateTime))
                return (T)(object)(Value);
            else if (typeof(T) == typeof(string))
            {
                if (IsURL)
                    throw new Exception("DataTreeValue is an URL, not a raw string!");

                var str = Value as string;

                if (str == null)
                    return default(T);

                if (str.Length > 1 && str[0] == '@')
                    return (T)(object)str.Substring(1);

                return (T)(object)str;
            }
            else if (typeof(T) == typeof(Uri))
                return (T)(object)ExtractURL();
            else if (typeof(T) == typeof(ulong))
            {
                if (Value is long i64)
                    return (T)(object)unchecked((ulong)i64);
                else
                    return default;
            }
            else if(typeof(T) == typeof(uint))
            {
                if (Value is uint u32)
                    return (T)(object)u32;
                else if (Value is int i32)
                    return (T)(object)unchecked((uint)i32);
                else if (Value is long i64)
                    return (T)(object)unchecked((uint)i64);
                else
                    return default;
            }
            else if (typeof(T) == typeof(TimeSpan))
                return (T)(object)new TimeSpan(Extract<long>());
            else
                return (T)(Value.ToType(typeof(T), null));
        }

        public Uri TryExtractURL()
        {
            if (Value == null)
                return null;

            if (!IsURL)
                return null;

            var str = (string)Value;
            str = str.Substring(1);

            if (!Uri.IsWellFormedUriString(str, UriKind.Absolute))
                return null;

            return new Uri(str);
        }

        Uri ExtractURL()
        {
            if (Value == null)
                return null;

            var str = (string)Value;

            if (!IsURL)
                throw new Exception("DataTreeValue isn't an URL: " + str);

            return new Uri(str.Substring(1));
        }

        public DataTreeValue(Uri url)
        {
            UpdateValue(url);
        }

        public DataTreeValue(TimeSpan timespan)
        {
            this.Value = timespan.Ticks;
        }

        public void UpdateValue(Uri url)
        {
            //this.Primitive = DataPrimitiveType.String;

            if (url == null)
                this.Value = null;
            else 
                this.Value = '@' + url.ToString();
        }

        public static DataTreeValue FromEnum<E>(E val)
        {
            var enumType = typeof(E);

            if (!enumType.IsEnum)
                throw new Exception("Type isn't an enumeration!");

            return new DataTreeValue(val.ToString());
        }

        public E ExtractEnum<E>()
        {
            var enumType = typeof(E);

            if (!enumType.IsEnum)
                throw new Exception("Type isn't an enumeration!");

            return (E)Enum.Parse(enumType, Extract<string>());
        }

        string PreprocessString(string str)
        {
            if(str != null && str.Length > 1)
            {
                if (str[0] == '@')
                    return "@" + str;
            }

            return str;
        }

        IConvertible Preprocess(IConvertible val)
        {
            switch(val)
            {
                case string str:
                    return PreprocessString(str);

                default:
                    return val;
            }
        }

        public void  UpdateValue(IConvertible val)
        {
            this.Value = Preprocess(val);
        }

        public DataTreeValue(IConvertible val)
        {
            this.Value = Preprocess(val);
        }

        public DataTreeValue(string str)
        {
            this.Value = PreprocessString(str);
        }

        public static DataTreeValue RawString(string str)
        {
            var node = new DataTreeValue();
            //node.Primitive = DataPrimitiveType.String;
            node.Value = str;
            return node;
        }

        public override string ToString() => $"DataTreeValue: {Value}";
    }
}
