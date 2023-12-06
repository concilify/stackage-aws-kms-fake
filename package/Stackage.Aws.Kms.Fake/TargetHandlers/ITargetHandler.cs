using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public interface ITargetHandler
{
   bool CanHandle(string target);

   Task<IResult> HandleAsync(HttpContext context);
}
