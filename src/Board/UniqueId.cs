namespace TalesOfTribute;

public class UniqueId
{
    public int Value { get; }

    private static int _counter = 10000;

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
        if (left is null) return right is null;
        if (right is null) return false;

        return left.Value == right.Value;
    }

    public static bool operator !=(UniqueId? left, UniqueId? right)
    {
        return !(left == right);
    }
}
