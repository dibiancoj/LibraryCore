using System;

namespace LibraryCore.Core.DataTypes.Unions
{
    /// <summary>
    /// Union type between 2 different data types
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public abstract class UnionBase
    {
        protected Type? UnionType { get; init; }
        protected dynamic? TValue { get; init; }

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
                     default(T);
        }

    }
}
