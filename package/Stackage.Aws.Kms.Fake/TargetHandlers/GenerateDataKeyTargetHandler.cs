using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Exceptions;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class GenerateDataKeyTargetHandler : TargetHandlerBase
{
   private readonly IKeyStore _keyStore;
   private readonly IAuthenticationContext _authenticationContext;

   public GenerateDataKeyTargetHandler(
      IKeyStore keyStore,
      IAuthenticationContext authenticationContext)
   {
      _keyStore = keyStore;
      _authenticationContext = authenticationContext;
   }

   protected override string Target => "TrentService.GenerateDataKey";

   public override async Task<IResult> HandleAsync(HttpContext context)
   {
      var request = await ParseAmazonJsonAsync<GenerateDataKeyRequest>(context.Request);

      var key = _keyStore.GetOne(request.KeyId);

      if (key == null)
      {
         throw new NotFoundException(
            $"Key 'arn:aws:kms:{_authenticationContext.Region}:000000000000:key/{request.KeyId}' does not exist");
      }

      // TODO: KeySpec: AES_256 | AES_128
      // TODO: NumberOfBytes: 1 => 1024

      var dataKey = new AesGcmCipher().

   }
}
