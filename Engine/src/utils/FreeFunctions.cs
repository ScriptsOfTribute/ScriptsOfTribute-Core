using System.Security.Cryptography;

namespace ScriptsOfTribute.utils;

public static class FreeFunctions
{
    public static ulong ScrambleSeed(ulong seed)
    {
        var seedBytes = BitConverter.GetBytes(seed);
        using var sha256 = SHA256.Create();
        var data = sha256.ComputeHash(seedBytes);
        return BitConverter.ToUInt64(data);
    }
}
