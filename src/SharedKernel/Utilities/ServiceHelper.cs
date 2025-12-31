using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using SharedKernel.ErrorEntity;

namespace SharedKernel.Utilities;

public static partial class ServiceHelper
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    public static string GeneratePassword(int length)
    {
        var secret = new StringBuilder();
        while (length-- > 0)
        {
            secret.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);
        }
        return secret.ToString();
    }

    public static string GenerateSecretCode(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        }

        var buffer = new StringBuilder(length);
        while (buffer.Length < length)
        {
            buffer.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);
        }

        return buffer.ToString();
    }

    public static bool IsBase64String(string input)
    {
        var buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer, out _);
    }

    public static async Task<T> DecryptRequest<T>(string requestData, string secretKey, string iv)
    {
        string decryptedRequest = AesEncryption.Decrypt(requestData, secretKey, iv);
        if (ContainsScriptTag(decryptedRequest))
        {
            throw new CustomException("Bad request");
        }

        T deserializedRequest = DeserializeFromJson<T>(decryptedRequest);

        return deserializedRequest;
    }

    public static string SerializeAsJson<T>(T item) => JsonSerializer.Serialize(item);

    public static T DeserializeFromJson<T>(string input) => JsonSerializer.Deserialize<T>(input);

    public static string GenerateRandomNumber(int numbersOfDigit)
    {
        byte[] data = new byte[8];
        RandomNumberGenerator.Fill(data);
        long generatedValue = BitConverter.ToInt64(data, 0);
        string digitIdentifier = "1" + new string('0', numbersOfDigit);
        // Ensure the value is positive and within the desired range
        long randomNumber = Math.Abs(generatedValue % long.Parse(digitIdentifier, CultureInfo.InvariantCulture));
        return randomNumber.ToString($"D{numbersOfDigit}", CultureInfo.InvariantCulture);
    }

    public static int GenerateSecureRandomNumber(int maxValue, int minValue = 0)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minValue), "minValue cannot be greater than maxValue.");
        }

        byte[] uint32Buffer = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(uint32Buffer);
        uint randomValue = BitConverter.ToUInt32(uint32Buffer, 0);
        return (int)(randomValue % (maxValue - minValue + 1)) + minValue;
    }

    public static string? AesJsonEncryption(string plainText, string secretKey, string Iv)
    {   
        return AesEncryption.Encrypt(plainText, secretKey, Iv);
    }


    public static bool ContainsScriptTag(string input) =>
        !string.IsNullOrWhiteSpace(input) && CheckScript().IsMatch(input);

    [GeneratedRegex(@"<\s*script[^>]*>(.*?)<\s*/\s*script\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex CheckScript();

    public static string MaskSensitiveInfo(string input)
    {
        input ??= " ";
        input = MaskBVN().Replace(input, "*******");
        input = MaskDateOfBirth().Replace(input, "*******");
        input = NIN().Replace(input, "*******");
        input = MaskPassword().Replace(input, "*******");
        //[GeneratedRegex(@"(?<=\\?""bvn\\?\"":\\?"")[\d]+(?=\\?"")", RegexOptions.IgnoreCase)]
        return input;
    }

    [GeneratedRegex(@"(?<=""bvn"":\s*"")[^""]+", RegexOptions.IgnoreCase)]
    private static partial Regex MaskBVN();
    
    [GeneratedRegex(@"(?<=""dateOfBirth"":\s*"")[^""]+", RegexOptions.IgnoreCase)]
    private static partial Regex MaskDateOfBirth();
    
    [GeneratedRegex(@"(?<=""password"":\s*"")[^""]+", RegexOptions.IgnoreCase)]
    private static partial Regex MaskPassword();
    
    [GeneratedRegex(@"(?<=""nin"":\s*"")[^""]+", RegexOptions.IgnoreCase)]
    private static partial Regex NIN();
}
