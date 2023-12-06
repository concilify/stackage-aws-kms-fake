using System.Collections.Generic;
using System.Linq;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;

namespace Stackage.Aws.Kms.Fake.Tests.TestDoubles;

public class FakeKeyStore : IKeyStore
{
   public List<Key> Keys { get; } = new();

   public void Add(Key key)
   {
      Keys.Add(key);
   }

   public IReadOnlyList<Key> GetByRegion(string region)
   {
      return Keys.Where(k => k.Region == region).ToList();
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
