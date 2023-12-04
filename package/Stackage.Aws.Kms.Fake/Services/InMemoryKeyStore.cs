using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Services;

public class InMemoryKeyStore : IKeyStore
{
   private readonly ConcurrentDictionary<Guid, Key> _keys = new();

   public void Add(Key key)
   {
      if (!_keys.TryAdd(key.Id, key))
      {
         throw new InvalidOperationException("A key with the same Id already exists");
      }
   }

   public IReadOnlyList<Key> GetByRegion(string region)
   {
      return _keys.Values.Where(k => k.Region == region).ToList();
   }
}
