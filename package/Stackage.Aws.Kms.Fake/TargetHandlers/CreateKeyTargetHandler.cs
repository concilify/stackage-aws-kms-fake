using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class CreateKeyTargetHandler : ITargetHandler
{
   private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = null };

    private static readonly Regex CredentialRegex =
        new(@"Credential=(?<aws_secret_id>.*?)\/(?<date>[0-9]{8})\/(?<region>.*?)\/kms\/aws4_request");

    private readonly IGenerateGuids _guidGenerator;
    private readonly IKeyStore _keyStore;

    public CreateKeyTargetHandler(
       IGenerateGuids guidGenerator,
       IKeyStore keyStore)
    {
       _guidGenerator = guidGenerator;
       _keyStore = keyStore;
    }

    public bool CanHandle(string target) => target == "TrentService.CreateKey";

    public IResult Handle(HttpContext context)
    {
        var credentialMatch = CredentialRegex.Match(context.Request.Headers.Authorization.ToString());

        if (!credentialMatch.Success)
        {
            return Results.Unauthorized();
        }

        var key = Key.Create(
           id: _guidGenerator.Generate(),
           region: credentialMatch.Groups["region"].Value);

        _keyStore.Add(key);

        var keyMetadata = new KeyMetadata(
            AwsAccountId: "000000000000",
            KeyId: key.Id.ToString(),
            key.Arn,
            CreationDate: $"{key.CreatedAt:yyyy-MM-ddTHH:mm:ss.ffffffzzz}",
            Enabled: true,
            Description: string.Empty,
            KeyUsage: "ENCRYPT_DECRYPT",
            KeyState: "Enabled",
            Origin: "AWS_KMS",
            KeyManager: "CUSTOMER",
            CustomerMasterKeySpec: "SYMMETRIC_DEFAULT",
            KeySpec: "SYMMETRIC_DEFAULT",
            EncryptionAlgorithms: new[] { "SYMMETRIC_DEFAULT" },
            MultiRegion: false);

        return Results.Json(
            new CreateKeyResponse(keyMetadata),
            options: SerializerOptions,
            contentType: "application/x-amz-json-1.1",
            statusCode: 200);
    }
}
