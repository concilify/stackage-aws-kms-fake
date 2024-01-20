using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stackage.Aws.Kms.Fake.Model;

public class Key
{
   private const string DefaultRegion = "eu-central-1";

   private readonly ICipher _cipher;
   private readonly KeyMaterial _keyMaterial;

   private ImmutableList<string> _aliases;

   private Key(
      Guid id,
      string region,
      DateTime createdAt,
      string[] aliases,
      ICipher cipher,
      KeyMaterial keyMaterial)
   {
      Id = id;
      Region = region;
      CreatedAt = createdAt;
      _aliases = ImmutableList.Create(aliases);
      _cipher = cipher;
      _keyMaterial = keyMaterial;
   }

   public Guid Id { get; }

   public string Region { get; }

   public IReadOnlyList<string> Aliases => _aliases;

   public DateTime CreatedAt { get; }

   public string Arn => $"arn:aws:kms:{Region}:000000000000:key/{Id}";

   public static Key Create(
      Guid? id = null,
      string? region = null,
      string[]? aliases = null,
      ICipher? cipher = null,
      KeyMaterial? keyMaterial = null)
   {
      cipher ??= new AesGcmCipher();

      if (keyMaterial != null && !cipher.IsValid(keyMaterial))
      {
         throw new ArgumentOutOfRangeException(nameof(keyMaterial), "Invalid length");
      }

      return new Key(
         id ?? new Guid(),
         region ?? DefaultRegion,
         DateTime.Now,
         aliases ?? Array.Empty<string>(),
         cipher,
         keyMaterial ?? cipher.GenerateKeyMaterial());
   }

   public void AddAlias(string aliasName)
   {
      _aliases = _aliases.Add(aliasName);
   }

   public byte[] Encrypt(ReadOnlySpan<byte> plaintext)
   {
      return _cipher.Encrypt(plaintext, _keyMaterial);
   }

   public byte[] Decrypt(ReadOnlySpan<byte> ciphertext)
   {
      return _cipher.Decrypt(ciphertext, _keyMaterial);
   }
}
