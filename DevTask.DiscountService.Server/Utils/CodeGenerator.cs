using System.Security.Cryptography;
using System.Text;

namespace DevTask.DiscountService.Server.Utils;

public static class CodeGenerator
{
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var randomBytes = new byte[length];

        var builder = new StringBuilder(length);
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        foreach (byte b in randomBytes)
        {
            builder.Append(chars[b % chars.Length]);
        }
        return  builder.ToString();
    }
}