using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class DecryptTargetHandler : ITargetHandler
{
   public bool CanHandle(string target) => target == "TrentService.Decrypt";

   public IResult Handle(HttpContext context)
   {
      throw new System.NotImplementedException();
   }
}
