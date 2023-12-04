using System;
using System.Collections.Generic;

namespace Stackage.Aws.Kms.Fake.TargetHandlers.Model;

public record ListKeysResponse(IReadOnlyList<ListKeysResponse.KeyDto> Keys)
{
   public record KeyDto(Guid KeyId, string KeyArn);
};
