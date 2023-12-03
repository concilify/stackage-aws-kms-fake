using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public interface ITargetHandler
{
   bool CanHandle(string target);

   IResult Handle(HttpContext context);
}
