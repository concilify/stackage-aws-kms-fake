using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.Tests.Stubs;

namespace Stackage.Aws.Kms.Fake.Tests.TargetScenarios
{
   // ReSharper disable once InconsistentNaming
   public class list_keys
   {
      private Guid _awsRequestId;
      private Guid _keyAId;
      private Guid _keyBId;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _awsRequestId = Guid.NewGuid();
         _keyAId = Guid.NewGuid();
         _keyBId = Guid.NewGuid();

         var guidGenerator = new StubGuidGenerator(_awsRequestId);

         var keyStore = new StubKeyStore
         {
            Keys =
            {
               Key.Create(_keyAId, "RegionA"),
               Key.Create(_keyBId, "RegionB")
            }
         };

         await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton<IGenerateGuids>(guidGenerator);
                  services.AddSingleton<IKeyStore>(keyStore);
               });
            });
         using var httpClient = application.CreateClient();

         var request = new HttpRequestMessage(HttpMethod.Post, "/")
         {
            Content = JsonContent.Create(new { }),
            Headers =
            {
               Authorization = new AuthenticationHeaderValue(
                  "AWS4-HMAC-SHA256",
                  "Credential=ValidAwsSecretId/20001231/RegionA/kms/aws4_request, SignedHeaders=ValidSignedHeaders, Signature=ValidSignature")
            }
         };

         request.Headers.Add("X-Amz-Target", "TrentService.ListKeys");

         _response = await httpClient.SendAsync(request);
      }

      [Test]
      public void endpoint_returns_200_okay()
      {
         Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task endpoint_returns_content()
      {
         var contentJson = await _response.Content.ReadAsStringAsync();

         var content = JsonNode.Parse(contentJson);

         var keys = content?["Keys"];

         Assert.That(
            keys?[0]?["KeyId"]?.GetValue<string>(),
            Is.EqualTo(_keyAId.ToString()));
         Assert.That(
            keys?[0]?["KeyArn"]?.GetValue<string>(),
            Is.EqualTo($"arn:aws:kms:RegionA:000000000000:key/{_keyAId}"));
      }

      [Test]
      public void endpoint_returns_content_type()
      {
         Assert.That(_response.Content.Headers.ContentType?.ToString(), Is.EqualTo("application/x-amz-json-1.1"));
      }

      [Test]
      public void endpoint_returns_request_id()
      {
         Assert.That(_response.Headers.GetValues("X-Amzn-RequestId").Single(), Is.EqualTo(_awsRequestId.ToString()));
      }
   }
}
