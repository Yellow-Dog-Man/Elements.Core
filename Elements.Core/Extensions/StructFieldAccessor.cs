using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cloudtoid;

namespace Elements.Core
{
    public class StructFieldMissingException : Exception
    {
        public Type RootType { get; private set; }
        public Type StructType { get; private set; }
        public string FieldName { get; private set; }

        public StructFieldMissingException(Type rootType, Type structType, string fieldName)
        {
            this.RootType = rootType;
            this.StructType = structType;
            this.FieldName = fieldName;
        }
    }

    public interface IStructMemberProxy
    {
        Type MemberType { get; }
        object GetValue(object current);
        object SetValue(object current, object value);
    }

    public class StructFieldProxy : IStructMemberProxy
    {
        FieldInfo field;

        public StructFieldProxy(FieldInfo field)
        {
            this.field = field;
        }

        public Type MemberType => this.field.FieldType;

        public object GetValue(object current) => field.GetValue(current);

        public object SetValue(object current, object value)
        {
            field.SetValue(current, value);
            return current;
        }
    }

    public class NullableFieldProxy : IStructMemberProxy
    {
        public Type MemberType { get; private set; }

        public NullableFieldProxy(Type nullableType)
        {
            if (!nullableType.IsNullable())
                throw new ArgumentException($"Type must be a nullable type!");

            MemberType = Nullable.GetUnderlyingType(nullableType);
        }

        public object GetValue(object current) => current;
        public object SetValue(object current, object value) => value;
    }

    public class StructPropertyMethodProxy : IStructMemberProxy
    {
        PropertyInfo property;
        MethodInfo setMethod;

        public StructPropertyMethodProxy(PropertyInfo property, MethodInfo setMethod)
        {
            this.property = property;
            this.setMethod = setMethod;
        }

        public Type MemberType => property.PropertyType;

        public object GetValue(object current) => property.GetValue(current);

        public object SetValue(object current, object value) => setMethod.Invoke(current, new object[] { value });
    }

    public class StructMemberAccessor
    {
        public Type RootType { get; private set; }
        public Type TargetType { get; private set; }

        List<IStructMemberProxy> hierarchy;

        public StructMemberAccessor(Type rootType, string path)
        {
            RootType = rootType;

            hierarchy = new List<IStructMemberProxy>();

            GenerateHierarchy(rootType, path);
        }

        public object Get(object root)
        {
            // go down the hierachy, extracting values
            foreach (var info in hierarchy)
            {
                if (root == null)
                    break;

                root = info.GetValue(root);
            }

            return root;
        }

        public object Set(object target, object value)
        {
            return InternalSet(target, value);
        }

        object InternalSet(object target, object value, int level = 0)
        {
            if (level == hierarchy.Count)
                return value;

            var levelInfo = hierarchy[level];

            var subValue = levelInfo.GetValue(target);

            subValue = InternalSet(subValue, value, level + 1);

            target = levelInfo.SetValue(target, subValue);

            return target;
        }

        void GenerateHierarchy(Type type, string path)
        {
            // Done generating
            if (string.IsNullOrEmpty(path))
            {
                TargetType = type;
                return;
            }

            // Skip the initial . in case it's there
            // This is to allow generic code, which might append a member to previous path, which can be empty
            // meaning it should access the top level member
            if (path[0] == '.')
                path = path.Substring(1);

            // TODO!!!! 
            // Don't do any checking here? Have the target code handle it instead?
            /*if(!type.IsValueType && type != typeof(string) && type != typeof(Uri) && type )
                throw new Exception($"Member {path} of {RootType} is Type {type} which is not supported type!");*/

            var dotIndex = path.IndexOf(".");

            string field;

            if (dotIndex < 0)
            {
                field = path;
                path = null;
            }
            else
            {
                field = path.Substring(0, dotIndex);
                path = path.Substring(dotIndex + 1);
            }

            IStructMemberProxy proxy = null;

            if (type.IsNullable())
            {
                if (field != "value")
                    throw new ArgumentException($"Nullable types must use the \"value\" field name to access the value");

                proxy = new NullableFieldProxy(type);
            }
            else
            {
                var fieldInfo = type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null)
                    proxy = new StructFieldProxy(fieldInfo);
                else
                {
                    var propertyInfo = type.GetProperty(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var methodInfo = type.GetMethod($"Set{field.ToUpper()}", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (propertyInfo != null && methodInfo != null)
                        proxy = new StructPropertyMethodProxy(propertyInfo, methodInfo);
                }
            }

            if (proxy == null)
                throw new StructFieldMissingException(RootType, type, field);

            hierarchy.Add(proxy);

            // run recursively on the remainder of the path
            GenerateHierarchy(proxy.MemberType, path);
        }
    }
}
