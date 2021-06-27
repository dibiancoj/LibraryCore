namespace LibraryCore.Core.DataTypes
{
    /// <summary>
    /// A basic data type to allow a mutable key value pair
    /// </summary>
    /// <typeparam name="TKey">Key data type</typeparam>
    /// <typeparam name="TValue">Value data type</typeparam>
    public class MutableKeyValuePair<TKey, TValue>
    {
        /// <summary>
        /// Default constructor. Key and Value will be assigned the default value for the given data type
        /// </summary>
        public MutableKeyValuePair()
        {
        }

        public MutableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; } = default!;
        public TValue Value { get; set; } = default!;
    }
}
