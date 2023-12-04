using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.Tests.Stubs;

namespace Stackage.Aws.Kms.Fake.Tests.TargetScenarios
{
   // ReSharper disable once InconsistentNaming
   public class list_keys_unauthorized
   {
      private Guid _awsRequestId;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _awsRequestId = Guid.NewGuid();

         var guidGenerator = new StubGuidGenerator(_awsRequestId);

         await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton<IGenerateGuids>(guidGenerator);
               });
            });
         using var httpClient = application.CreateClient();

         var request = new HttpRequestMessage(HttpMethod.Post, "/")
         {
            Content = JsonContent.Create(new { })
         };

         request.Headers.Add("X-Amz-Target", "TrentService.ListKeys");

         _response = await httpClient.SendAsync(request);
      }

      [Test]
      public void endpoint_returns_401_unauthorized()
      {
         Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      }

      [Test]
      public async Task endpoint_returns_empty()
      {
         var content = await _response.Content.ReadAsStringAsync();

         Assert.That(content, Is.Empty);
      }
   }
}
