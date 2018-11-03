namespace CryptogramLibrary
{
  internal static class Cryptography
  {
    public static byte[] Encrypt(byte[] data, byte[] password)
    {
      System.Security.Cryptography.HashAlgorithm hashType = new System.Security.Cryptography.SHA256Managed();
      var hash = hashType.ComputeHash(password);
      var result = new byte[data.Length];
      var p = 0;
      for (var i = 0; i < data.Length; i++)
      {
        result[i] = (byte)(data[i] ^ hash[p]);
        p += 1;
        if (p != hash.Length) continue;
        p = 0;
        hash = hashType.ComputeHash(hash);
      }
      return result;
    }

    public static byte[] Decrypt(byte[] encryptedData, byte[] password)
    {
      return Encrypt(encryptedData, password);
    }
  }
}
