using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IGenerateGuids, GuidGenerator>();
builder.Services.AddTransient<ITargetHandler, CreateKeyTargetHandler>();

var app = builder.Build();

app.MapPost("/", ([FromHeader(Name = "X-Amz-Target")] string target, HttpContext context) =>
{
   var guidGenerator = context.RequestServices.GetRequiredService<IGenerateGuids>();

   context.Response.Headers.Append("X-Amzn-RequestId", guidGenerator.Generate().ToString());

   var targetHandlers = context.RequestServices.GetServices<ITargetHandler>();

   var targetHandler = targetHandlers.SingleOrDefault(h => h.CanHandle(target));

   if (targetHandler == null)
   {
      return Results.BadRequest("Invalid X-Amz-Target");
   }

   var result = targetHandler.Handle(context);

   return result;
});

await app.RunAsync();
