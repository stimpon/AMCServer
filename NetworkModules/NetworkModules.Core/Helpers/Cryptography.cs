/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Core
{
    // Namespaces
    using System.Security.Cryptography;

    /// <summary>
    /// Contains crypto helper funtions
    /// </summary>
    public static class Cryptography
    {
        /// <summary>
        /// Creates the aes encryptor (Standard encryptor for this library).
        /// </summary>
        /// <param name="genKey">if set to <c>true</c> [gen key].</param>
        /// <param name="genIV">if set to <c>true</c> [gen iv].</param>
        /// <returns>The new encryptor</returns>
        public static AesCryptoServiceProvider CreateAesEncryptor(bool genKey = true, bool genIV = true)
        {
            // Create encryptor
            var FileEncryptor = new AesCryptoServiceProvider();
            FileEncryptor.Mode = CipherMode.CBC;
            FileEncryptor.KeySize = 128;
            FileEncryptor.BlockSize = 128;
            FileEncryptor.FeedbackSize = 128;
            FileEncryptor.Padding = PaddingMode.PKCS7;

            // If key or IV should be generated
            if(genIV || genKey)
                // Create random byte generator
                using (var RNG = RandomNumberGenerator.Create())
                {
                    if(genKey) RNG.GetBytes(FileEncryptor.Key);
                    if(genIV) RNG.GetBytes(FileEncryptor.IV);
                }

            // Return the encryptor
            return FileEncryptor;
        }
    }
}
