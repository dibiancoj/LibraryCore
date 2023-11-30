namespace LibraryCore.Core.EnumUtilities;

public class BitMaskBuilder<T>(T initialValue) where T : struct, Enum
{
    public T Value { get; private set; } = initialValue;

    public BitMaskBuilder<T> AddItem(T valueToAdd)
    {
        Value = EnumUtility.BitMaskAddItem(Value, valueToAdd);
        return this;
    }

    public BitMaskBuilder<T> RemoveItem(T valueToRemove)
    {
        Value = EnumUtility.BitMaskRemoveItem(Value, valueToRemove);
        return this;
    }

    public bool ContainsValue(T valueToCheckFor) => EnumUtility.BitMaskContainsValue(Value, valueToCheckFor);

    public IEnumerable<T> SelectedItems() => EnumUtility.BitMaskSelectedItems(Value);
}
