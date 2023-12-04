using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class ListKeysTargetHandler : TargetHandlerBase
{
    private readonly IAuthenticationContext _authenticationContext;
    private readonly IKeyStore _keyStore;

    public ListKeysTargetHandler(
       IAuthenticationContext authenticationContext,
       IKeyStore keyStore)
    {
       _authenticationContext = authenticationContext;
       _keyStore = keyStore;
    }

    protected override string Target => "TrentService.ListKeys";

    public override IResult Handle(HttpContext context)
    {
        var keys = _keyStore.GetByRegion(_authenticationContext.Region);

        var keysDto = keys.Select(k => new ListKeysResponse.KeyDto(k.Id, k.Arn)).ToImmutableList();

        return AmazonJson(new ListKeysResponse(keysDto));
    }
}
