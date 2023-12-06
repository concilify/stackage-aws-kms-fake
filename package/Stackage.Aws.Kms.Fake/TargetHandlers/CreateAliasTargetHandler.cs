using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Exceptions;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class CreateAliasTargetHandler : TargetHandlerBase
{
   private readonly IKeyStore _keyStore;
   private readonly IAuthenticationContext _authenticationContext;

   public CreateAliasTargetHandler(
      IKeyStore keyStore,
      IAuthenticationContext authenticationContext)
   {
      _keyStore = keyStore;
      _authenticationContext = authenticationContext;
   }

   protected override string Target => "TrentService.CreateAlias";

   public override async Task<IResult> HandleAsync(HttpContext context)
   {
      var request = await ParseAmazonJsonAsync<CreateAliasRequest>(context.Request);

      var key = _keyStore.GetOne(request.TargetKeyId);

      if (key == null)
      {
         throw new NotFoundException(
            $"Key 'arn:aws:kms:{_authenticationContext.Region}:000000000000:key/{request.TargetKeyId}' does not exist");
      }

      key.AddAlias(request.AliasName);

      return AmazonJson(new { });
   }
}
