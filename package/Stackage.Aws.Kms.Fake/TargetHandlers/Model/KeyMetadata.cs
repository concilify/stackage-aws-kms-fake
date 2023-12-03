using System.Text.Json.Serialization;

namespace Stackage.Aws.Kms.Fake.TargetHandlers.Model;

public record KeyMetadata(
   [property: JsonPropertyName("AWSAccountId")]
   string AwsAccountId,
   string KeyId,
   string Arn,
   string CreationDate,
   bool Enabled,
   string Description,
   string KeyUsage,
   string KeyState,
   string Origin,
   string KeyManager,
   string CustomerMasterKeySpec,
   string KeySpec,
   string[] EncryptionAlgorithms,
   bool MultiRegion
);
