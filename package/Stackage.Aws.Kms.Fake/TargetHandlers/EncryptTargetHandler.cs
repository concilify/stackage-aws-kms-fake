using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class EncryptTargetHandler : TargetHandlerBase
{
   private readonly IKeyStore _keyStore;

   public EncryptTargetHandler(IKeyStore keyStore)
   {
      _keyStore = keyStore;
   }

   protected override string Target => "TrentService.Encrypt";

   public override async Task<IResult> HandleAsync(HttpContext context)
   {
      // TODO: Check header

      var request = await JsonSerializer.DeserializeAsync<EncryptRequest>(context.Request.Body);

      if (request == null)
      {
         throw new InvalidOperationException("The request body is missing");
      }

      // TODO: Validation

      var key = _keyStore.GetOne(request.KeyId);

      if (key == null)
      {
         throw new InvalidOperationException("The key was not found");
      }

      var ciphertextBlob = Encrypt(key, request.Plaintext);

      return AmazonJson(new EncryptResponse(ciphertextBlob, key.Arn));
   }

   private static string Encrypt(Key key, string plaintext)
   {
      var keyId = Encoding.UTF8.GetBytes(key.Id.ToString("d"));
      var ciphertext = key.Encrypt(Convert.FromBase64String(plaintext));

      // TODO: Possibly some more preamble
      return $"{Convert.ToBase64String(keyId)}{Convert.ToBase64String(ciphertext)}";
   }
}
