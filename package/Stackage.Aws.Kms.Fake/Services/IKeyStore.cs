﻿using System;
using System.Collections.Generic;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Services;

public interface IKeyStore
{
   void Add(Key key);

   Key? GetOne(Guid id);

   IReadOnlyList<Key> GetAll();
}
