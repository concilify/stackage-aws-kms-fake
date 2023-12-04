using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class ListKeysTargetHandler : ITargetHandler
{
   private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = null };

    private static readonly Regex CredentialRegex =
        new(@"Credential=(?<aws_secret_id>.*?)\/(?<date>[0-9]{8})\/(?<region>.*?)\/kms\/aws4_request");

    private readonly IKeyStore _keyStore;

    public ListKeysTargetHandler(IKeyStore keyStore)
    {
       _keyStore = keyStore;
    }

    public bool CanHandle(string target) => target == "TrentService.ListKeys";

    public IResult Handle(HttpContext context)
    {
        var credentialMatch = CredentialRegex.Match(context.Request.Headers.Authorization.ToString());

        if (!credentialMatch.Success)
        {
            return Results.Unauthorized();
        }

        var keys = _keyStore.GetByRegion(credentialMatch.Groups["region"].Value);

        var keysDto = keys.Select(k => new ListKeysResponse.KeyDto(k.Id, k.Arn)).ToImmutableList();

        return Results.Json(
            new ListKeysResponse(keysDto),
            options: SerializerOptions,
            contentType: "application/x-amz-json-1.1",
            statusCode: 200);
    }
}
