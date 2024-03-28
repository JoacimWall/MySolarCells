using System.Security.Cryptography;

namespace MySolarCells.Helpers;

public static class StringHelper
{
    public static string Decrypt(this string encryptedValue, string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptedValue))
            return string.Empty;

        string iv = encryptedValue.Substring(encryptedValue.IndexOf(';') + 1, encryptedValue.Length - encryptedValue.IndexOf(';') - 1);
        encryptedValue = encryptedValue.Substring(0, encryptedValue.IndexOf(';'));

        return AesDecryptStringFromBytes(Convert.FromBase64String(encryptedValue), CreateKey(encryptionKey), Convert.FromBase64String(iv));
    }
    public static string Encrypt(this string clearValue, string encryptionKey)
    {
        using Aes aes = Aes.Create();
        aes.Key = CreateKey(encryptionKey);

        byte[] encrypted = AesEncryptStringToBytes(clearValue, aes.Key, aes.IV);
        return Convert.ToBase64String(encrypted) + ";" + Convert.ToBase64String(aes.IV);
    }
    public static string EncodeTo64(string toEncode)
    {
        byte[] toEncodeAsBytes
              = System.Text.Encoding.ASCII.GetBytes(toEncode);
        string returnValue
              = Convert.ToBase64String(toEncodeAsBytes);
        return returnValue;
    }
    public static string DecodeFrom64(string encodedData)
    {
        byte[] encodedDataAsBytes
            = Convert.FromBase64String(encodedData);
        string returnValue =
           System.Text.Encoding.ASCII.GetString(encodedDataAsBytes);
        return returnValue;
    }


    private static byte[] CreateKey(string password, int keyBytes = 32)
    {
        byte[] salt = new byte[] { 23, 22, 45, 52, 90, 32, 32, 10 };
        int iterations = 300;
#pragma warning disable SYSLIB0041
        var keyGenerator = new Rfc2898DeriveBytes(password, salt, iterations);
#pragma warning restore SYSLIB0041
        return keyGenerator.GetBytes(keyBytes);
    }

    private static byte[] AesEncryptStringToBytes(string plainText, byte[] key, byte[] iv)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException($"{nameof(plainText)}");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException($"{nameof(key)}");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException($"{nameof(iv)}");

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using MemoryStream memoryStream = new MemoryStream();
        using (ICryptoTransform encryptor = aes.CreateEncryptor())
        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plainText);
        }
        var encrypted = memoryStream.ToArray();

        return encrypted;
    }
    private static string AesDecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
    {
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException($"{nameof(cipherText)}");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException($"{nameof(key)}");
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException($"{nameof(iv)}");

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using MemoryStream memoryStream = new MemoryStream(cipherText);
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new StreamReader(cryptoStream);
        var plaintext = streamReader.ReadToEnd();
        return plaintext;
    }



}

