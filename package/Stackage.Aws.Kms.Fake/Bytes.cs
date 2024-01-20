using System;
using System.Security.Cryptography;

namespace Stackage.Aws.Kms.Fake;

internal static class Bytes
{
   private static readonly int GuidSizeInBytes = Guid.Empty.ToByteArray().Length;

   public static byte[] GenerateRandomBytes(int sizeBytes)
   {
      var backingKey = new byte[sizeBytes];
      RandomNumberGenerator.Fill(backingKey);

      return backingKey;
   }

   public static byte[] Concatenate(byte[] from1, byte[] from2, byte[] from3)
   {
      var to = new byte[from1.Length + from2.Length + from3.Length];

      BlockCopy(from1, to, toOffset: 0);
      BlockCopy(from2, to, toOffset: from1.Length);
      BlockCopy(from3, to, toOffset: from1.Length + from2.Length);

      return to;
   }

   public static byte[] Join(Guid id, byte[] ciphertext)
   {
      var buffer = new byte[GuidSizeInBytes + ciphertext.Length];

      BlockCopy(id.ToByteArray(), buffer, toOffset: 0);
      BlockCopy(ciphertext, buffer, toOffset: GuidSizeInBytes);

      return buffer;
   }

   public static (Guid, byte[]) Split(byte[] buffer)
   {
      var id = new byte[GuidSizeInBytes];
      var ciphertext = new byte[buffer.Length - GuidSizeInBytes];

      BlockCopy(buffer, fromOffset: 0, id);
      BlockCopy(buffer, fromOffset: GuidSizeInBytes, ciphertext);

      return (new Guid(id), ciphertext);
   }

   private static void BlockCopy(byte[] from, byte[] to, int toOffset)
   {
      Buffer.BlockCopy(from, 0, to, toOffset, from.Length);
   }

   private static void BlockCopy(byte[] from, int fromOffset, byte[] to)
   {
      Buffer.BlockCopy(from, fromOffset, to, 0, to.Length);
   }
}
