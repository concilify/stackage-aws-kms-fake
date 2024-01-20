using System;
using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Stackage.Aws.Kms.Fake.Model;

internal class AesGcmCipher : ICipher
{
   private const int KeyMaterialSizeInBytes = 32;
   private const int NonceSizeInBytes = 12;
   private const int TagSizeInBytes = 16;

   public bool IsValid(KeyMaterial keyMaterial)
   {
      if (keyMaterial == null)
      {
         throw new ArgumentNullException(nameof(keyMaterial));
      }

      return keyMaterial.Bytes.Length != KeyMaterialSizeInBytes;
   }

   public KeyMaterial GenerateKeyMaterial()
   {
      return new KeyMaterial(Bytes.GenerateRandomBytes(KeyMaterialSizeInBytes).ToImmutableArray());
   }

   public byte[] Encrypt(ReadOnlySpan<byte> plaintext, KeyMaterial keyMaterial)
   {
      using var aes = CreateAesGcm(keyMaterial);

      var nonce = Bytes.GenerateRandomBytes(NonceSizeInBytes);
      var actualCiphertext = new byte[plaintext.Length];
      var tag = new byte[TagSizeInBytes];

      aes.Encrypt(nonce, plaintext, actualCiphertext, tag);

      return Bytes.Concatenate(nonce, actualCiphertext, tag);
   }

   public byte[] Decrypt(ReadOnlySpan<byte> ciphertext, KeyMaterial keyMaterial)
   {
      using var aes = CreateAesGcm(keyMaterial);

      var nonce = ciphertext[..NonceSizeInBytes];
      var actualCiphertext = ciphertext.Slice(NonceSizeInBytes, ciphertext.Length - NonceSizeInBytes - TagSizeInBytes);
      var tag = ciphertext[^TagSizeInBytes..];

      var plaintext = new byte[actualCiphertext.Length];

      aes.Decrypt(nonce, actualCiphertext, tag, plaintext);

      return plaintext;
   }

   private AesGcm CreateAesGcm(KeyMaterial keyMaterial)
   {
      if (!IsValid(keyMaterial))
      {
         throw new ArgumentOutOfRangeException(nameof(keyMaterial), "Invalid length");
      }

#if NET6_0
      return new AesGcm(keyMaterial.Bytes.AsSpan());
#else
      return new AesGcm(keyMaterial.Bytes.AsSpan(), TagSizeInBytes);
#endif
   }
}
