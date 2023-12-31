﻿using System;
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

   public byte[] Encrypt(ReadOnlySpan<byte> plaintext)
   {
      using var aes = CreateAesGcm();

      var nonce = GenerateRandomBytes(NonceSizeInBytes);
      var actualCiphertext = new byte[plaintext.Length];
      var tag = new byte[TagSizeInBytes];

      aes.Encrypt(nonce, plaintext, actualCiphertext, tag);

      return Bytes.Concatenate(nonce, actualCiphertext, tag);
   }

   public byte[] Decrypt(ReadOnlySpan<byte> ciphertext)
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
