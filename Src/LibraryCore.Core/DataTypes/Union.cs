using System;

namespace LibraryCore.Core.DataTypes
{

    /// <summary>
    /// Union type between 2 different data types
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class Union<T1, T2>
    {
        /// <summary>
        /// Create a union with the first type
        /// </summary>
        /// <param name="t1Value">Value of the T1 value</param>
        public Union(T1 t1Value)
        {
            UnionType = typeof(T1);
            TValue = t1Value;
        }

        /// <summary>
        /// Create a union with the second type
        /// </summary>
        /// <param name="t2Value"></param>
        public Union(T2 t2Value)
        {
            UnionType = typeof(T2);
            TValue = t2Value;
        }

        private Type UnionType { get; init; }
        private dynamic? TValue { get; init; }

        /// <summary>
        /// Returns true if the union contains a value of type T
        /// </summary>
        /// <remarks>The type of T must exactly match the type</remarks>
        public bool Is<T>() => typeof(T) == UnionType;

        /// <summary>
        /// Returns the union value cast to the given type.
        /// </summary>
        /// <remarks>Returns the value for the given type if it matches T. If the value does not match the the default value of T is returned</remarks>
        public T? As<T>()
        {
            return Is<T>() ?
                     TValue as dynamic :
                     default;
        }

    }
}
