using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Exceptions;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class DecryptTargetHandler : TargetHandlerBase
{
   private readonly IKeyStore _keyStore;
   private readonly IAuthenticationContext _authenticationContext;

   public DecryptTargetHandler(
      IKeyStore keyStore,
      IAuthenticationContext authenticationContext)
   {
      _keyStore = keyStore;
      _authenticationContext = authenticationContext;
   }

   protected override string Target => "TrentService.Decrypt";

   public override async Task<IResult> HandleAsync(HttpContext context)
   {
      var request = await ParseAmazonJsonAsync<DecryptRequest>(context.Request);

      var buffer = Convert.FromBase64String(request.CiphertextBlob);
      var (keyId, ciphertext) = Bytes.Split(buffer);

      var key = _keyStore.GetOne(keyId);

      if (key == null)
      {
         throw new NotFoundException(
            $"Key 'arn:aws:kms:{_authenticationContext.Region}:000000000000:key/{keyId}' does not exist");
      }

      var plaintext = Decrypt(key, ciphertext);

      return AmazonJson(new DecryptResponse(key.Arn, plaintext));
   }

   private static string Decrypt(Key key, byte[] ciphertext)
   {
      var plaintext = key.Decrypt(ciphertext);

      return Convert.ToBase64String(plaintext);
   }
}
