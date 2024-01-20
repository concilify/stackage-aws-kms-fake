using System;

namespace Stackage.Aws.Kms.Fake.Model;

public interface ICipher
{
   bool IsValid(KeyMaterial keyMaterial);

   KeyMaterial GenerateKeyMaterial();

   byte[] Encrypt(ReadOnlySpan<byte> plaintext, KeyMaterial keyMaterial);

   byte[] Decrypt(ReadOnlySpan<byte> ciphertext, KeyMaterial keyMaterial);
}
