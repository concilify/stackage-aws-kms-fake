using System;

namespace Stackage.Aws.Kms.Fake.TargetHandlers.Model;

public record CreateAliasRequest(string AliasName, Guid TargetKeyId);
