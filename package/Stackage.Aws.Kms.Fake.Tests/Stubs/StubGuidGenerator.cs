using System;
using System.Collections.Generic;
using Stackage.Aws.Kms.Fake.Services;

namespace Stackage.Aws.Kms.Fake.Tests.Stubs;

public class StubGuidGenerator : IGenerateGuids
{
   private readonly Queue<Guid> _ids;

   public StubGuidGenerator(params Guid[] ids)
   {
      _ids = new Queue<Guid>(ids);
   }

   public Guid Generate()
   {
      return _ids.Dequeue();
   }
}
