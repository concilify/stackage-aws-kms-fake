using System;

namespace Stackage.Aws.Kms.Fake.Exceptions;

internal abstract class AmazonErrorException : Exception
{
   protected AmazonErrorException(string message) : base(message)
   {
   }
}
