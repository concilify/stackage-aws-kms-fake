using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IKeyStore, InMemoryKeyStore>();

builder.Services.AddScoped<IAuthenticationContext, AuthenticationContext>();

builder.Services.AddTransient<IGenerateGuids, GuidGenerator>();
builder.Services.AddTransient<ITargetHandler, CreateKeyTargetHandler>();
builder.Services.AddTransient<ITargetHandler, ListKeysTargetHandler>();
builder.Services.AddTransient<ITargetHandler, EncryptTargetHandler>();
builder.Services.AddTransient<ITargetHandler, DecryptTargetHandler>();

var app = builder.Build();

// TODO: https://dev.to/chiragdm/envelope-encryption-using-aws-cli-3ejd
// TODO: encrypt data key
// TODO: decrypt data key
// TODO: create data key to tease backing keys
// TODO: Allow seeding of keys

app.Use(async (context, next) =>
{
   var authenticationContext = context.RequestServices.GetRequiredService<IAuthenticationContext>();

   if (!authenticationContext.IsAuthenticated)
   {
      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

      return;
   }

   await next(context);
});

app.MapPost("/", ([FromHeader(Name = "X-Amz-Target")] string target, HttpContext context) =>
{
   var guidGenerator = context.RequestServices.GetRequiredService<IGenerateGuids>();
   var targetHandlers = context.RequestServices.GetServices<ITargetHandler>();

   context.Response.Headers.Append("X-Amzn-RequestId", guidGenerator.Generate().ToString());

   var targetHandler = targetHandlers.SingleOrDefault(h => h.CanHandle(target));

   if (targetHandler == null)
   {
      return Results.BadRequest("Invalid X-Amz-Target header");
   }

   var result = targetHandler.Handle(context);

   return result;
});

await app.RunAsync();
