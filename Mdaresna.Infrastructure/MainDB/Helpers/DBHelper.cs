using System.Security.Cryptography;
using System.Text;

namespace Mdaresna.Infrastructure.MainDB.Helpers;

internal static class DBHelper
{
    public static string GenerateDbPassword(int length = 24)
    {
        if (length < 12)
            throw new ArgumentException("Password length should be at least 12 characters.", nameof(length));

        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()-_=+[]{}";

        string allChars = upper + lower + digits + special;

        var password = new StringBuilder();

        // Ensure password contains at least one char from each group
        password.Append(GetRandomChar(upper));
        password.Append(GetRandomChar(lower));
        password.Append(GetRandomChar(digits));
        password.Append(GetRandomChar(special));

        // Fill remaining characters
        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        // Shuffle password
        return Shuffle(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        int index = RandomNumberGenerator.GetInt32(chars.Length);
        return chars[index];
    }

    private static string Shuffle(string value)
    {
        var chars = value.ToCharArray();

        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
