using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.TargetHandlers;

public abstract class TargetHandlerBase : ITargetHandler
{
   private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = null };

   private const string AmazonJsonContentType = "application/x-amz-json-1.1";

   public bool CanHandle(string target) => target == Target;

   public abstract Task<IResult> HandleAsync(HttpContext context);

   protected abstract string Target { get; }

   protected static async Task<TRequest> ParseAmazonJsonAsync<TRequest>(HttpRequest httpRequest)
   {
      if (httpRequest.ContentType != AmazonJsonContentType)
      {
         throw new InvalidOperationException($"The Content-Type header must be {AmazonJsonContentType}.");
      }

      var request = await JsonSerializer.DeserializeAsync<TRequest>(httpRequest.Body);

      return request ?? throw new InvalidOperationException("The request body is missing.");
   }

   protected static IResult AmazonJson<TResponse>(TResponse response)
   {
      return Results.Json(
         response,
         options: SerializerOptions,
         contentType: AmazonJsonContentType,
         statusCode: 200);
   }
}
