using System;
using System.Security.Cryptography;

namespace Stackage.Aws.Kms.Fake.Model;

public class Key
{
   private const string DefaultRegion = "eu-central-1";
   private const int BackingKeySizeInBytes = 32;
   private const int NonceSizeInBytes = 12;
   private const int TagSizeInBytes = 16;

   private Key(Guid id, string region, byte[] backingKey, DateTime createdAt)
   {
      Id = id;
      Region = region;
      BackingKey = backingKey;
      CreatedAt = createdAt;
   }

   public Guid Id { get; }

   public string Region { get; }

   public byte[] BackingKey { get; }

   public DateTime CreatedAt { get; }

   public string Arn => $"arn:aws:kms:{Region}:000000000000:key/{Id}";

   public static Key Create(
      Guid? id = null,
      string? region = null,
      byte[]? backingKey = null)
   {
      if (backingKey != null && backingKey.Length != 32)
      {
         throw new ArgumentOutOfRangeException(nameof(backingKey), "Invalid length");
      }

      return new Key(
         id ?? new Guid(),
         region ?? DefaultRegion,
         backingKey ?? GenerateRandomBytes(BackingKeySizeInBytes),
         DateTime.Now);
   }

   // https://www.scottbrady91.com/c-sharp/aes-gcm-dotnet
   public ReadOnlySpan<byte> Encrypt(ReadOnlySpan<byte> plaintext)
   {
      using var aes = CreateAesGcm();

      var nonce = GenerateRandomBytes(NonceSizeInBytes);
      var actualCiphertext = new byte[plaintext.Length];
      var tag = new byte[TagSizeInBytes];

      aes.Encrypt(nonce, plaintext, actualCiphertext, tag);

      var ciphertext = new byte[nonce.Length + actualCiphertext.Length + tag.Length];

      BlockCopy(nonce, offset: 0);
      BlockCopy(actualCiphertext, offset: nonce.Length);
      BlockCopy(tag, offset: nonce.Length + actualCiphertext.Length);

      return ciphertext;

      void BlockCopy(byte[] source, int offset)
      {
         Buffer.BlockCopy(source, 0, ciphertext, offset, source.Length);
      }
   }

   public ReadOnlySpan<byte> Decrypt(ReadOnlySpan<byte> ciphertext)
   {
      using var aes = CreateAesGcm();

      var nonce = ciphertext[..NonceSizeInBytes];
      var actualCiphertext = ciphertext.Slice(NonceSizeInBytes, ciphertext.Length - NonceSizeInBytes - TagSizeInBytes);
      var tag = ciphertext[^TagSizeInBytes..];

      var plaintext = new byte[actualCiphertext.Length];

      aes.Decrypt(nonce, actualCiphertext, tag, plaintext);

      return plaintext;
   }

   private static byte[] GenerateRandomBytes(int sizeBytes)
   {
      var backingKey = new byte[sizeBytes];
      RandomNumberGenerator.Fill(backingKey);

      return backingKey;
   }

#if NET6_0
   private AesGcm CreateAesGcm() => new AesGcm(BackingKey);
#else
   private AesGcm CreateAesGcm() => new AesGcm(BackingKey, TagSizeInBytes);
#endif
}
