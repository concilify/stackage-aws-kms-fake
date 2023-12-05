using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public abstract class TargetHandlerBase : ITargetHandler
{
   private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = null };

   public bool CanHandle(string target) => target == Target;

   public abstract Task<IResult> HandleAsync(HttpContext context);

   protected abstract string Target { get; }

   protected static IResult AmazonJson<TResponse>(TResponse response)
   {
      return Results.Json(
         response,
         options: SerializerOptions,
         contentType: "application/x-amz-json-1.1",
         statusCode: 200);
   }
}
