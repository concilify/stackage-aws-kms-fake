using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.Tests.Stubs;

namespace Stackage.Aws.Kms.Fake.Tests.TargetScenarios;

public class encrypt_decrypt_roundtrip
{
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

}
