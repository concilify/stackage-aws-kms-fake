using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class ListKeysTargetHandler : TargetHandlerBase
{
    private readonly IKeyStore _keyStore;

    public ListKeysTargetHandler(IKeyStore keyStore)
    {
       _keyStore = keyStore;
    }

    protected override string Target => "TrentService.ListKeys";

    public override Task<IResult> HandleAsync(HttpContext context)
    {
        var keys = _keyStore.GetAll();

        var keysDto = keys.Select(k => new ListKeysResponse.KeyDto(k.Id, k.Arn)).ToImmutableList();

        return Task.FromResult(AmazonJson(new ListKeysResponse(keysDto)));
    }
}
