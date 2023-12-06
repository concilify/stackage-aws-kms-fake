using System;
using System.Collections.Generic;
using Stackage.Aws.Kms.Fake.Services;

namespace Stackage.Aws.Kms.Fake.Tests.Stubs;

public class StubGuidGenerator : IGenerateGuids
{
   private readonly Queue<Guid> _ids = new();

   public Guid Generate()
   {
      return _ids.Count != 0 ? _ids.Dequeue() : Guid.NewGuid();
   }

   public void Seed(params Guid[] ids)
   {
      foreach (var id in ids)
      {
         _ids.Enqueue(id);
      }
   }

   public void Clear()
   {
      _ids.Clear();
   }
}
