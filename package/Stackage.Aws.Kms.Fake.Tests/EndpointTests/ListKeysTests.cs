using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests
{
   public class ListKeysTests : EndpointScenarioBase
   {
      [Test]
      public async Task endpoint_returns_200_okay()
      {
         var httpResponse = await InvokeAsync("ListKeys");

         Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task endpoint_returns_content()
      {
         var keyAId = Guid.NewGuid();
         var keyBId = Guid.NewGuid();

         KeyStore.Seed(
            Key.Create(id: keyAId, region: "RegionA"),
            Key.Create(id: keyBId, region: "RegionB"));

         var httpResponse = await InvokeAsync(
            "ListKeys", authorization: new DefaultAuthorization("RegionA"));
         var content = await ReadAsJsonNode(httpResponse);

         var keys = content["Keys"];

         Assert.That(
            keys?[0]?["KeyId"]?.GetValue<string>(),
            Is.EqualTo(keyAId.ToString()));
         Assert.That(
            keys?[0]?["KeyArn"]?.GetValue<string>(),
            Is.EqualTo($"arn:aws:kms:RegionA:000000000000:key/{keyAId}"));
      }

      [Test]
      public async Task endpoint_returns_content_type()
      {
         var httpResponse = await InvokeAsync("ListKeys");

         Assert.That(httpResponse.Content.Headers.ContentType?.ToString(), Is.EqualTo("application/x-amz-json-1.1"));
      }

      [Test]
      public async Task endpoint_returns_request_id()
      {
         var awsRequestId = Guid.NewGuid();

         GuidGenerator.Seed(awsRequestId);

         var httpResponse = await InvokeAsync("ListKeys");

         Assert.That(httpResponse.Headers.GetValues("X-Amzn-RequestId").Single(), Is.EqualTo(awsRequestId.ToString()));
      }

      [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
      public async Task endpoint_returns_401_unauthorized_when_authorisation_header_is_invalid(IAuthorization authorization)
      {
         var httpResponse = await InvokeAsync("ListKeys", authorization: authorization);

         Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
      }

      [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
      public async Task endpoint_returns_empty_when_authorisation_header_is_invalid(IAuthorization authorization)
      {
         var httpResponse = await InvokeAsync("ListKeys", authorization: authorization);

         var content = await httpResponse.Content.ReadAsStringAsync();

         Assert.That(content, Is.Empty);
      }
   }
}
