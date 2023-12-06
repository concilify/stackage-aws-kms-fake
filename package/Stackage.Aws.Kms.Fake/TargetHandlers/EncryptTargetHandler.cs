using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Exceptions;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class EncryptTargetHandler : TargetHandlerBase
{
   private readonly IKeyStore _keyStore;
   private readonly IAuthenticationContext _authenticationContext;

   public EncryptTargetHandler(
      IKeyStore keyStore,
      IAuthenticationContext authenticationContext)
   {
      _keyStore = keyStore;
      _authenticationContext = authenticationContext;
   }

   protected override string Target => "TrentService.Encrypt";

   public override async Task<IResult> HandleAsync(HttpContext context)
   {
      var request = await ParseAmazonJsonAsync<EncryptRequest>(context.Request);

      var key = _keyStore.GetOne(request.KeyId);

      if (key == null)
      {
         throw new NotFoundException(
            $"Key 'arn:aws:kms:{_authenticationContext.Region}:000000000000:key/{request.KeyId}' does not exist");
      }

      var ciphertextBlob = Encrypt(key, request.Plaintext);

      return AmazonJson(new EncryptResponse(ciphertextBlob, key.Arn));
   }

   private static string Encrypt(Key key, string plaintext)
   {
      var ciphertext = key.Encrypt(Convert.FromBase64String(plaintext));

      var buffer = Bytes.Join(key.Id, ciphertext);

      return Convert.ToBase64String(buffer);
   }
}
