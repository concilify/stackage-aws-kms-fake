using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class DecryptTargetHandler : TargetHandlerBase
{
   private static readonly int KeyIdBase64SizeInBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("d"))).Length;

   private readonly IKeyStore _keyStore;

   public DecryptTargetHandler(IKeyStore keyStore)
   {
      _keyStore = keyStore;
   }

   protected override string Target => "TrentService.Decrypt";

   public override async Task<IResult> HandleAsync(HttpContext context)
   {
      // TODO: Check header

      var request = await JsonSerializer.DeserializeAsync<DecryptRequest>(context.Request.Body);

      if (request == null)
      {
         throw new InvalidOperationException("The request body is missing");
      }

      // TODO: error handling
      var keyId = request.CiphertextBlob[..KeyIdBase64SizeInBytes];
      var ciphertext = request.CiphertextBlob[KeyIdBase64SizeInBytes..];

      var key = _keyStore.GetOne(Guid.ParseExact(Encoding.UTF8.GetString(Convert.FromBase64String(keyId)), "d"));

      if (key == null)
      {
         throw new InvalidOperationException("The key was not found");
      }

      var plaintext = Decrypt(key, ciphertext);

      return AmazonJson(new DecryptResponse(key.Arn, plaintext));
   }

   private static string Decrypt(Key key, string ciphertext)
   {
      var plaintext = key.Decrypt(Convert.FromBase64String(ciphertext));

      return Convert.ToBase64String(plaintext);
   }
}
