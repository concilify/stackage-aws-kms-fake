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
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.Tests.Stubs;

namespace Stackage.Aws.Kms.Fake.Tests.TargetScenarios
{
   // ReSharper disable once InconsistentNaming
   public class create_key
   {
      private Guid _awsRequestId;
      private Guid _keyId;
      private StubKeyStore _keyStore;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _awsRequestId = Guid.NewGuid();
         _keyId = Guid.NewGuid();
         _keyStore = new StubKeyStore();

         var guidGenerator = new StubGuidGenerator(_awsRequestId, _keyId);

         await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton<IGenerateGuids>(guidGenerator);
                  services.AddSingleton<IKeyStore>(_keyStore);
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
                  "Credential=ValidAwsSecretId/20001231/ArbitraryRegion/kms/aws4_request, SignedHeaders=ValidSignedHeaders, Signature=ValidSignature")
            }
         };

         request.Headers.Add("X-Amz-Target", "TrentService.CreateKey");

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

         var keyMetadata = content?["KeyMetadata"];

         Assert.That(
            keyMetadata?["KeyId"]?.GetValue<string>(),
            Is.EqualTo(_keyId.ToString()));
         Assert.That(
            keyMetadata?["Arn"]?.GetValue<string>(),
            Is.EqualTo($"arn:aws:kms:ArbitraryRegion:000000000000:key/{_keyId}"));
         Assert.That(
            keyMetadata?["CreationDate"]?.GetValue<DateTime>(),
            Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(5)));
         Assert.That(
            keyMetadata?["Enabled"]?.GetValue<bool>(),
            Is.True);
         Assert.That(
            keyMetadata?["KeyState"]?.GetValue<string>(),
            Is.EqualTo("Enabled"));
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

      [Test]
      public void endpoint_adds_key_to_store()
      {
         Assert.That(_keyStore.Keys.Count, Is.EqualTo(1));
         Assert.That(_keyStore.Keys[0].Id, Is.EqualTo(_keyId));
         Assert.That(_keyStore.Keys[0].Region, Is.EqualTo("ArbitraryRegion"));
      }
   }
}
