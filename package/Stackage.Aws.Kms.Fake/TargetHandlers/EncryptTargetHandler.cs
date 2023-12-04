using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public class EncryptTargetHandler : ITargetHandler
{
   public bool CanHandle(string target) => target == "TrentService.Encrypt";

   public IResult Handle(HttpContext context)
   {
      throw new System.NotImplementedException();
   }
}
