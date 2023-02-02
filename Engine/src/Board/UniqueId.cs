namespace ScriptsOfTribute;

public readonly struct UniqueId
{
    private const int InitialValue = 10000;
    public int Value { get; }

    private static int _counter = InitialValue;

    private UniqueId(int value)
    {
        Value = value;
    }

    public static UniqueId Create()
    {
        return new UniqueId(_counter++);
    }

    public static UniqueId Empty => new(-1);

    public static explicit operator int(UniqueId id) => id.Value;

    public static bool operator ==(UniqueId? left, UniqueId? right)
    {
        return left?.Value == right?.Value;
    }

    public static bool operator !=(UniqueId? left, UniqueId? right)
    {
        return !(left == right);
    }

    public static UniqueId FromExisting(int id)
    {
        if (id < InitialValue || id >= _counter)
        {
            throw new ArgumentException("Card with this ID hasn't been generated yet.");
        }

        return new UniqueId(id);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
