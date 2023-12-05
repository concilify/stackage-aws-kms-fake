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

   public IReadOnlyList<Key> GetAll(string region)
   {
      return Keys.Where(k => k.Region == region).ToList();
   }
}
