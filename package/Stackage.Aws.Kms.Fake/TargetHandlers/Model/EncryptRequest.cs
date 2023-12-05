using System;

namespace Stackage.Aws.Kms.Fake.TargetHandlers.Model;

public record EncryptRequest(Guid KeyId, string Plaintext);
