using System;
using System.Collections.Generic;
using System.Linq;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;

namespace Stackage.Aws.Kms.Fake.Tests.Stubs;

public class StubKeyStore : IKeyStore
{
   public List<Key> Keys { get; } = new();

   public void Add(Key key)
   {
      Keys.Add(key);
   }

   public Key GetOne(Guid id)
   {
      return Keys.SingleOrDefault(k => k.Id == id);
   }

   public IReadOnlyList<Key> GetAll()
   {
      return Keys;
   }

   public void Seed(params Key[] keys)
   {
      foreach (var key in keys)
      {
         Keys.Add(key);
      }
   }

   public void Clear()
   {
      Keys.Clear();
   }
}
