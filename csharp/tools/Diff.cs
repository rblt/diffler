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
    /// Represents a helper class for detecting different property values on two instance of the same type.
    /// </summary>
    public static class Diff
    {
        #region Diff

        #region Types

        private static class ArrayEqualityComparer
        {
            public static IEqualityComparer<T[]> Create<T>(IEqualityComparer<T> comparer)
            {
                return new ArrayEqualityComparer<T>(comparer);
            }

            public static IEqualityComparer<T[]> Create<T>()
            {
                return new ArrayEqualityComparer<T>();
            }
        }

        private sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
        {
            private static readonly IEqualityComparer<T[]> defaultInstance = new
                ArrayEqualityComparer<T>();

            public static IEqualityComparer<T[]> Default
            {
                get { return defaultInstance; }
            }

            private readonly IEqualityComparer<T> elementComparer;

            public ArrayEqualityComparer()
                : this(EqualityComparer<T>.Default)
            {
            }

            public ArrayEqualityComparer(IEqualityComparer<T> elementComparer)
            {
                this.elementComparer = elementComparer;
            }

            public bool Equals(T[] x, T[] y)
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

        private delegate IDictionary<string, object> ComparerDelagate<T>(T a, T b);
        private delegate bool ArrayComparerDelegate(object aArrayOfT, object bArrayOfT);

        #endregion

        #region Membervariables

        private static Dictionary<Type, Delegate> _comparers = null;
        private static Dictionary<Type, ArrayComparerDelegate> _arrayComparers = null;

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
        public static bool ArraysAreEqual(Type type, object v1, object v2)
        {
            if (_arrayComparers == null)
            {
                lock (_syncRoot2)
                {
                    if (_arrayComparers == null)
                        _arrayComparers = new Dictionary<Type, ArrayComparerDelegate>();
                }
            }

            ArrayComparerDelegate compareArrays = null;
            if (!_arrayComparers.ContainsKey(type))
            {
                lock (_syncRoot2)
                {
                    if (!_arrayComparers.ContainsKey(type))
                    {
                        var arrayType = type.MakeArrayType();
                        var tyArrayComparer = Assembly.GetExecutingAssembly().GetType(typeof(Diff).FullName + "+ArrayEqualityComparer");
                        var createMethod = tyArrayComparer.GetMethod("Create", new Type[] { }).MakeGenericMethod(type);
                        var comparer = createMethod.Invoke(null, null);
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

                        _arrayComparers[type] = compareArrays;
                    }
                }
            }
            else compareArrays = _arrayComparers[type];

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
                                .Select(p => new { Property = p, IsArray = p.PropertyType.IsArray && p.PropertyType.HasElementType })
                                .Select(p =>
                                   Expression.IfThen(
                                       Expression.Not(
                                           Expression.Call(
                                               p.IsArray ? _arraysAreEqual : _areEqual,
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
