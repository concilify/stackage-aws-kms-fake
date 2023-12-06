namespace Stackage.Aws.Kms.Fake.Exceptions;

internal class NotFoundException : AmazonErrorException
{
   public NotFoundException(string message) : base(message)
   {
   }
}
