using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Services;

public class InMemoryKeyStore : IKeyStore
{
   private readonly ConcurrentDictionary<Guid, Key> _keys;
   private readonly IAuthenticationContext _authenticationContext;

   public InMemoryKeyStore(
      ConcurrentDictionary<Guid, Key> keys,
      IAuthenticationContext authenticationContext)
   {
      _keys = keys;
      _authenticationContext = authenticationContext;
   }

   public void Add(Key key)
   {
      if (key.Region != _authenticationContext.Region)
      {
         throw new InvalidOperationException("Cannot store a key for another region.");
      }

      if (!_keys.TryAdd(key.Id, key))
      {
         throw new InvalidOperationException("A key with the same id already exists.");
      }
   }

   public Key? GetOne(Guid id)
   {
      return _keys.Values.SingleOrDefault(k => k.Id == id && k.Region == _authenticationContext.Region);
   }

   public Key? GetOne(string idOrAlias)
   {
      return _keys.Values.SingleOrDefault(k => (k.Id.ToString() == idOrAlias || k.Aliases.Contains(idOrAlias) ) && k.Region == _authenticationContext.Region);
   }

   public IReadOnlyList<Key> GetAll()
   {
      return _keys.Values.Where(k => k.Region == _authenticationContext.Region).ToList();
   }
}
