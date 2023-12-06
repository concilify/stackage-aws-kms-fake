using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Kms.Fake.Exceptions;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.TargetHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<ConcurrentDictionary<Guid, Key>>();

builder.Services.AddScoped<IAuthenticationContext, AuthenticationContext>();

builder.Services.AddTransient<IKeyStore, InMemoryKeyStore>();
builder.Services.AddTransient<IGenerateGuids, GuidGenerator>();
builder.Services.AddTransient<ITargetHandler, CreateKeyTargetHandler>();
builder.Services.AddTransient<ITargetHandler, CreateAliasTargetHandler>();
builder.Services.AddTransient<ITargetHandler, ListKeysTargetHandler>();
builder.Services.AddTransient<ITargetHandler, EncryptTargetHandler>();
builder.Services.AddTransient<ITargetHandler, DecryptTargetHandler>();

var app = builder.Build();

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

app.Use(async (context, next) =>
{
   var guidGenerator = context.RequestServices.GetRequiredService<IGenerateGuids>();

   context.Response.Headers.Append("X-Amzn-RequestId", guidGenerator.Generate().ToString());

   await next(context);
});

app.Use(async (context, next) =>
{
   try
   {
      await next(context);
   }
   catch (AmazonErrorException e)
   {
      context.Response.Headers.Append("X-Amzn-Errortype", e.GetType().Name);
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

      await context.Response.WriteAsJsonAsync(new Dictionary<string, string>
      {
         ["__type"] = e.GetType().Name,
         ["message"] = e.Message
      });
   }
});

app.MapPost("/", async ([FromHeader(Name = "X-Amz-Target")] string target, HttpContext context) =>
{
   var targetHandlers = context.RequestServices.GetServices<ITargetHandler>();

   var targetHandler = targetHandlers.SingleOrDefault(h => h.CanHandle(target));

   if (targetHandler == null)
   {
      return Results.BadRequest("Invalid X-Amz-Target header");
   }

   var result = await targetHandler.HandleAsync(context);

   return result;
});

await app.RunAsync();
