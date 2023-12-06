using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests
{
   public class CreateKeyTests : EndpointScenarioBase
   {
      [Test]
      public async Task endpoint_returns_200_okay()
      {
         var httpResponse = await InvokeAsync("CreateKey");

         Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task endpoint_returns_content()
      {
         var awsRequestId = Guid.NewGuid();
         var keyId = Guid.NewGuid();

         GuidGenerator.Seed(awsRequestId, keyId);

         var httpResponse = await InvokeAsync("CreateKey", authorization: new DefaultAuthorization("ArbitraryRegion"));
         var content = await ReadAsJsonNode(httpResponse);

         var keyMetadata = content?["KeyMetadata"];

         Assert.That(
            keyMetadata?["KeyId"]?.GetValue<string>(),
            Is.EqualTo(keyId.ToString()));
         Assert.That(
            keyMetadata?["Arn"]?.GetValue<string>(),
            Is.EqualTo($"arn:aws:kms:ArbitraryRegion:000000000000:key/{keyId}"));
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
      public async Task endpoint_returns_content_type()
      {
         var httpResponse = await InvokeAsync("CreateKey");

         Assert.That(httpResponse.Content.Headers.ContentType?.ToString(), Is.EqualTo("application/x-amz-json-1.1"));
      }

      [Test]
      public async Task endpoint_returns_request_id()
      {
         var awsRequestId = Guid.NewGuid();

         GuidGenerator.Seed(awsRequestId);

         var httpResponse = await InvokeAsync("CreateKey");

         Assert.That(httpResponse.Headers.GetValues("X-Amzn-RequestId").Single(), Is.EqualTo(awsRequestId.ToString()));
      }

      [Test]
      public async Task endpoint_adds_key_to_store()
      {
         var awsRequestId = Guid.NewGuid();
         var keyId = Guid.NewGuid();

         GuidGenerator.Seed(awsRequestId, keyId);

         await InvokeAsync("CreateKey", authorization: new DefaultAuthorization("ArbitraryRegion"));

         Assert.That(KeyStore.Keys.Count, Is.EqualTo(1));
         Assert.That(KeyStore.Keys[0].Id, Is.EqualTo(keyId));
         Assert.That(KeyStore.Keys[0].Region, Is.EqualTo("ArbitraryRegion"));
      }

      [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
      public async Task endpoint_returns_401_unauthorized_when_authorisation_header_is_invalid(IAuthorization authorization)
      {
         var httpResponse = await InvokeAsync("CreateKey", authorization: authorization);

         Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      }

      [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
      public async Task endpoint_returns_empty_when_authorisation_header_is_invalid(IAuthorization authorization)
      {
         var httpResponse = await InvokeAsync("CreateKey", authorization: authorization);

         var content = await httpResponse.Content.ReadAsStringAsync();

         Assert.That(content, Is.Empty);
      }
   }
}
