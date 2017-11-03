using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace rblt.Tools
{
    /// <summary>
    /// Represents an attribute for the property to be ignored when used with the Tools.Diff methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DiffIgnoreAttribute : Attribute
    {
        #region DiffIgnoreAttribute

        public DiffIgnoreAttribute()
        {
        }

        #endregion
    }

    /// <summary>
    /// Represents an attribute for an array type property of which element order is ignored when used with the Tools.Diff methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DiffIgnoreOrderAttribute : Attribute
    {
        #region DiffIgnoreOrderAttribute

        public DiffIgnoreOrderAttribute()
        {
        }

        #endregion
    }

    /// <summary>
    /// Represents a helper class for detecting different property values on two instance of the same type.
    /// </summary>
    public static class Diff
    {
        #region Diff

        #region Types

        private static class ArrayEqualityComparer
        {
            public static IEqualityComparer<T[]> Create<T>(IEqualityComparer<T> comparer, bool ignoreOrder = false)
            {
                if (ignoreOrder)
                    return new UnorderedArrayEqualityComparer<T>(comparer);
                        
                return new ArrayEqualityComparer<T>(comparer);
            }

            public static IEqualityComparer<T[]> Create<T>(bool ignoreOrder = false)
            {
                if (ignoreOrder)
                    return new UnorderedArrayEqualityComparer<T>();

                return new ArrayEqualityComparer<T>();
            }
        }

        private abstract class ArrayEqualityComparerBase<T>: IEqualityComparer<T[]>
        {
            protected readonly IEqualityComparer<T> elementComparer;

            public ArrayEqualityComparerBase()
                : this(EqualityComparer<T>.Default)
            {
            }

            public ArrayEqualityComparerBase(IEqualityComparer<T> elementComparer)
            {
                this.elementComparer = elementComparer;
            }

            public virtual bool Equals(T[] x, T[] y)
            {
                if (x == y)
                {
                    return true;
                }
               
                return false;
            }

            public int GetHashCode(T[] array)
            {
                if (array == null)
                {
                    return 0;
                }
                int hash = 23;
                foreach (T item in array)
                {
                    hash = hash * 31 + elementComparer.GetHashCode(item);
                }
                return hash;
            }
        }

        private sealed class UnorderedArrayEqualityComparer<T> : ArrayEqualityComparerBase<T>
        {
            private static readonly IEqualityComparer<T[]> defaultInstance = new
                UnorderedArrayEqualityComparer<T>();

            public static IEqualityComparer<T[]> Default
            {
                get { return defaultInstance; }
            }

            public UnorderedArrayEqualityComparer()
                : base()
            {
            }

            public UnorderedArrayEqualityComparer(IEqualityComparer<T> elementComparer)
                : base(elementComparer)
            {
            }

            public override bool Equals(T[] x, T[] y)
            {
                if (x == y)
                {
                    return true;
                }
                if (x == null || y == null)
                {
                    return false;
                }
                if (x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (IndexOf(y, x[i]) < 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            private int IndexOf(T[] array, T element)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (elementComparer.Equals(array[i], element))
                        return i;
                }

                return -1;
            }
        }

        private sealed class ArrayEqualityComparer<T> : ArrayEqualityComparerBase<T>
        {
            private static readonly IEqualityComparer<T[]> defaultInstance = new
                ArrayEqualityComparer<T>();

            private static readonly IEqualityComparer<T[]> orderIgnoreInstance = new
                UnorderedArrayEqualityComparer<T>();

            public static IEqualityComparer<T[]> Default
            {
                get { return defaultInstance; }
            }

            public static IEqualityComparer<T[]> IgnoreOrder
            {
                get { return orderIgnoreInstance; }
            }

            public ArrayEqualityComparer()
                : base()
            {
            }

            public ArrayEqualityComparer(IEqualityComparer<T> elementComparer)
                : base(elementComparer)
            {
            }

            public override bool Equals(T[] x, T[] y)
            {
                if (x == y)
                {
                    return true;
                }
                if (x == null || y == null)
                {
                    return false;
                }
                if (x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (!elementComparer.Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }
        
                return true;
            }
        }

        private delegate IDictionary<string, object> ComparerDelagate<T>(T a, T b);
        private delegate bool ArrayComparerDelegate(object aArrayOfT, object bArrayOfT);

        #endregion

        #region Membervariables

        private static Dictionary<Type, Delegate> _comparers = null;
        private static Dictionary<Type, ArrayComparerDelegate> _arrayComparers = null;
        private static Dictionary<Type, ArrayComparerDelegate> _arrayComparers_ignoreOrder = null;

        private static readonly object _syncRoot = new object();
        private static readonly object _syncRoot2 = new object();

        private static MethodInfo _areEqual = typeof(Diff).GetMethod("ObjectsAreEqual");
        public static bool ObjectsAreEqual(Type t, object v1, object v2)
        {
            if (v1 == null && v2 == null) return true;
            else if (v1 == null && v2 != null) return false;
            else if (v1 != null && v2 == null) return false;
            else return v1.Equals(v2);
        }

        private static MethodInfo _arraysAreEqual = typeof(Diff).GetMethod("ArraysAreEqual");
        public static bool ArraysAreEqual(Type type, object v1, object v2, bool ignoreOrder)
        {
            if (ignoreOrder)
                return ArraysAreEqualInternal(type, v1, v2, ignoreOrder, ref _arrayComparers_ignoreOrder);

            return ArraysAreEqualInternal(type, v1, v2, ignoreOrder, ref _arrayComparers);
        }
        private static bool ArraysAreEqualInternal(Type type, object v1, object v2, bool ignoreOrder, ref Dictionary<Type, ArrayComparerDelegate> arrayComparers)
        { 
            if (arrayComparers == null)
            {
                lock (_syncRoot2)
                {
                    if (arrayComparers == null)
                        arrayComparers = new Dictionary<Type, ArrayComparerDelegate>();
                }
            }

            ArrayComparerDelegate compareArrays = null;
            if (!arrayComparers.ContainsKey(type))
            {
                lock (_syncRoot2)
                {
                    if (!arrayComparers.ContainsKey(type))
                    {
                        var arrayType = type.MakeArrayType();
                        var tyArrayComparer = Assembly.GetExecutingAssembly().GetType(typeof(Diff).FullName + "+ArrayEqualityComparer");
                        var createMethod = tyArrayComparer.GetMethod("Create", new Type[] { typeof(bool) }).MakeGenericMethod(type);
                        var comparer = createMethod.Invoke(null, new object[] { ignoreOrder });
                        var equalsMethodInfo = comparer.GetType().GetMethod("Equals", new Type[] { type.MakeArrayType(), type.MakeArrayType() });

                        ParameterExpression parA = Expression.Parameter(typeof(object), "a");
                        ParameterExpression parB = Expression.Parameter(typeof(object), "b");

                        var returnTarget = Expression.Label(typeof(bool));
                        var returnLabel = Expression.Label(returnTarget, Expression.Constant(false));

                        compareArrays = Expression.Lambda<ArrayComparerDelegate>(
                            Expression.Block(
                                Expression.Return(
                                    returnTarget,
                                    Expression.Call(
                                        Expression.Constant(comparer),
                                        equalsMethodInfo,
                                        Expression.Convert(parA, arrayType),
                                        Expression.Convert(parB, arrayType)
                                    )
                                ),
                                returnLabel
                            ),
                            parA, parB
                        ).Compile();

                        arrayComparers[type] = compareArrays;
                    }
                }
            }
            else compareArrays = arrayComparers[type];

            return compareArrays(v1, v2);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the different property name and value pairs of the second instance compared to the first.
        /// </summary>
        /// <param name="a">An instance of T on which the comparision is based.</param>
        /// <param name="b">An instance of T which will be compared to the base instance.</param>
        /// <returns>A dictionary instance when containing differences, otherwise null.</returns>
        public static IDictionary<string, object> Them<T>(T a, T b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException(a == null ? "a" : "b", "Cannot compare null instances.");

            if (Object.ReferenceEquals(a, b) || a.Equals(b))
                return null;

            if (_comparers == null)
            {
                lock (_syncRoot)
                {
                    if (_comparers == null)
                        _comparers = new Dictionary<Type, Delegate>();
                }
            }

            ComparerDelagate<T> compare = null;
            if (!_comparers.ContainsKey(typeof(T)))
            {
                lock (_syncRoot)
                {
                    if (_comparers.ContainsKey(typeof(T))) compare = (ComparerDelagate<T>)_comparers[typeof(T)];
                    else
                    {
                        var props = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);

                        ParameterExpression parA = Expression.Parameter(typeof(T), "a");
                        ParameterExpression parB = Expression.Parameter(typeof(T), "b");

                        var changeSet = Expression.Variable(typeof(Dictionary<string, object>), "changeSet");
                        var tyChangeSet = typeof(Dictionary<string, object>);
                        var ci = tyChangeSet.GetConstructor(new Type[] { });
                        var addMethod = tyChangeSet.GetMethod("Add", new Type[] { typeof(string), typeof(object) });

                        var returnTarget = Expression.Label(typeof(IDictionary<string, object>));
                        var returnLabel = Expression.Label(returnTarget, Expression.Constant(null, typeof(Dictionary<string, object>)));

                        var body = Expression.Block(
                            new ParameterExpression[] { changeSet },
                            props.Where(p => !Attribute.IsDefined(p, typeof(DiffIgnoreAttribute)))
                                .Select(p => new { Property = p, IsArray = p.PropertyType.IsArray && p.PropertyType.HasElementType, IgnoreOrder = Attribute.IsDefined(p, typeof(DiffIgnoreOrderAttribute)) })
                                .Select(p =>
                                   Expression.IfThen(
                                       Expression.Not(
                                           p.IsArray ?
                                               Expression.Call(_arraysAreEqual,
                                                   Expression.Constant(p.Property.PropertyType.GetElementType(), typeof(Type)),
                                                   Expression.Convert(Expression.Property(parA, p.Property), typeof(object)),
                                                   Expression.Convert(Expression.Property(parB, p.Property), typeof(object)),
                                                   Expression.Constant(p.IgnoreOrder, typeof(bool))) :
                                               Expression.Call(
                                                   _areEqual,
                                                   Expression.Constant(p.Property.PropertyType.GetElementType(), typeof(Type)),
                                                   Expression.Convert(Expression.Property(parA, p.Property), typeof(object)),
                                                   Expression.Convert(Expression.Property(parB, p.Property), typeof(object))
                                               )
                                       ),
                                       Expression.Block(
                                           Expression.IfThen(Expression.Equal(changeSet, Expression.Constant(null, typeof(Dictionary<string, object>))),
                                               Expression.Assign(changeSet, Expression.New(ci))),
                                           Expression.Call(changeSet, addMethod, Expression.Constant(p.Property.Name), Expression.Convert(Expression.Property(parB, p.Property), typeof(object)))
                                       )
                                   )
                                ).Concat(
                                   new Expression[] { Expression.Return(returnTarget, changeSet), returnLabel }
                                )
                            );

                        compare = Expression.Lambda<ComparerDelagate<T>>(body, parA, parB).Compile();

                        _comparers[typeof(T)] = compare;
                    }
                }
            }
            else compare = (ComparerDelagate<T>)_comparers[typeof(T)];

            return compare(a, b);
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Returns the different property name and value pairs of the given instance compared to itself.
        /// </summary>
        /// <param name="that">The current instance of T on which the comparision is based.</param>
        /// <param name="andThat">An instance of T which will be compared to the current instance.</param>
        /// <returns>A dictionary instance when containing differences, otherwise null.</returns>
        public static IDictionary<string, object> Versus<T>(this T that, T andThat)
        {
            return Diff.Them(that, andThat);
        }

        #endregion

        #endregion
    }
}
