namespace Stackage.Aws.Kms.Fake.Services;

public interface IAuthenticationContext
{
   bool IsAuthenticated { get; }

   string Region { get; }
}
