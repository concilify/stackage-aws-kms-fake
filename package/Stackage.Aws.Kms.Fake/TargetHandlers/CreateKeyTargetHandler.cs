using Microsoft.AspNetCore.Http;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers.Model;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class CreateKeyTargetHandler : TargetHandlerBase
{
    private readonly IAuthenticationContext _authenticationContext;
    private readonly IGenerateGuids _guidGenerator;
    private readonly IKeyStore _keyStore;

    public CreateKeyTargetHandler(
       IAuthenticationContext authenticationContext,
       IGenerateGuids guidGenerator,
       IKeyStore keyStore)
    {
       _authenticationContext = authenticationContext;
       _guidGenerator = guidGenerator;
       _keyStore = keyStore;
    }

    protected override string Target => "TrentService.CreateKey";

    public override IResult Handle(HttpContext context)
    {
        var key = Key.Create(
           id: _guidGenerator.Generate(),
           region: _authenticationContext.Region);

        _keyStore.Add(key);

        var keyMetadata = new CreateKeyResponse.KeyMetadataDto(
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

        return AmazonJson(new CreateKeyResponse(keyMetadata));
    }
}
