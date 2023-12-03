using System;

namespace Stackage.Aws.Kms.Fake.Services;

public class GuidGenerator : IGenerateGuids
{
   public Guid Generate()
   {
      return Guid.NewGuid();
   }
}
