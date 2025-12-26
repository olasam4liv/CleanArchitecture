using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace SharedKernel.Utilities;

public static class AesEncryption
{
    public static string Decrypt(string ciphertext, string secretKey, string iv)
    {
        using var myAes = Aes.Create();
        myAes.Mode = CipherMode.CBC;
        myAes.Padding = PaddingMode.PKCS7;
        myAes.Key = Encoding.UTF8.GetBytes(secretKey);
        myAes.IV = Encoding.UTF8.GetBytes(iv);

        string roundtrip = DecryptStringFromBytes_Aes(ciphertext, myAes.Key, myAes.IV);
        return roundtrip;
    }

    public static string Encrypt(string plaintext, string secretkey, string iv)
    {
        using var myAes = Aes.Create();
        myAes.Mode = CipherMode.CBC;
        myAes.Padding = PaddingMode.PKCS7;

        myAes.Key = Encoding.UTF8.GetBytes(secretkey);
        myAes.IV = Encoding.UTF8.GetBytes(iv);

        byte[] encrypted = EncryptStringToBytes_Aes(plaintext, myAes.Key, myAes.IV);
        string ciphertext = ByteArrayToString(encrypted);
        return ciphertext;
    }

    private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        if (Key == null || Key.Length <= 0)
        {
            throw new ArgumentNullException(nameof(Key));
        }
            
        if (IV == null || IV.Length <= 0)
        {
            throw new ArgumentNullException(nameof(IV));
        }

        byte[] encrypted;
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            using ICryptoTransform encryptor = aesAlg.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            encrypted = msEncrypt.ToArray();
        }
        return encrypted;
    }

    private static string DecryptStringFromBytes_Aes(string cipherText, byte[] Key, byte[] IV)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);
        if (Key == null || Key.Length <= 0)
        {
            throw new ArgumentNullException(nameof(Key));  
        }
        if (IV == null || IV.Length <= 0)
        {  
            throw new ArgumentNullException(nameof(IV)); 
        }           

        string? plaintext = null;
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            using ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            byte[] cipherbytes = HexadecimalStringToByteArray(cipherText);

            using var msDecrypt = new MemoryStream(cipherbytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();
        }
        return plaintext ?? string.Empty;
    }

    private static byte[] HexadecimalStringToByteArray(string input)
    {
        int outputLength = input.Length / 2;
        byte[] output = new byte[outputLength];
        using (var sr = new StringReader(input))
        {
            for (int i = 0; i < outputLength; i++)
            {
                output[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
               
        }
        return output;
    }

    private static string ByteArrayToString(byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
        {
            hex.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
        }
        return hex.ToString();
    }
}
