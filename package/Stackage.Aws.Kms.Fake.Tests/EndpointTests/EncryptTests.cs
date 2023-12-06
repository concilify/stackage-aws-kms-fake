using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public class EncryptTests : EndpointScenarioBase
{
   private const string Region = "ArbitraryRegion";

   private Guid _keyId;

   protected override void SetupBeforeEachTest()
   {
      _keyId = Guid.NewGuid();

      KeyStore.Seed(Key.Create(id: _keyId, region: Region));
   }

   [Test]
   public async Task endpoint_returns_200_okay()
   {
      var httpResponse = await InvokeAsync(keyId: _keyId, plaintext: "Zm9v");

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
   }

   [Test]
   public async Task endpoint_returns_content()
   {
      var httpResponse = await InvokeAsync(keyId: _keyId, plaintext: "Zm9v");

      var content = await ReadAsJsonNode(httpResponse);

      Assert.That(content?["CiphertextBlob"]?.GetValue<string>(), Is.Not.Empty);
      Assert.That(content?["KeyId"]?.GetValue<string>(), Is.EqualTo($"arn:aws:kms:{Region}:000000000000:key/{_keyId}"));
   }

   [Test]
   public async Task endpoint_returns_content_type()
   {
      var httpResponse = await InvokeAsync(keyId: _keyId, plaintext: "Zm9v");

      Assert.That(httpResponse.Content.Headers.ContentType?.ToString(), Is.EqualTo("application/x-amz-json-1.1"));
   }

   [Test]
   public async Task endpoint_returns_request_id()
   {
      var awsRequestId = Guid.NewGuid();

      GuidGenerator.Seed(awsRequestId);

      var httpResponse = await InvokeAsync(keyId: _keyId, plaintext: "Zm9v");

      Assert.That(httpResponse.Headers.GetValues("X-Amzn-RequestId").Single(), Is.EqualTo(awsRequestId.ToString()));
   }

   [Test]
   public async Task endpoint_returns_400_when_key_not_found()
   {
      var httpResponse = await InvokeAsync(keyId: Guid.NewGuid(), plaintext: "Zm9v");

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
   }

   [Test]
   public async Task endpoint_returns_message_when_key_not_found()
   {
      var missingKeyId = Guid.NewGuid();

      var httpResponse = await InvokeAsync(
         keyId: missingKeyId,
         plaintext: "Zm9v",
         authorization: new DefaultAuthorization(Region));

      var content = await ReadAsJsonNode(httpResponse);

      Assert.That(
         content["__type"]?.GetValue<string>(),
         Is.EqualTo("NotFoundException"));
      Assert.That(
         content["message"]?.GetValue<string>(),
         Is.EqualTo($"Key 'arn:aws:kms:{Region}:000000000000:key/{missingKeyId}' does not exist"));
   }

   [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
   public async Task endpoint_returns_401_unauthorized_when_authorisation_header_is_invalid(IAuthorization authorization)
   {
      var httpResponse = await InvokeAsync(keyId: _keyId, plaintext: "Zm9v", authorization: authorization);

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
   }

   [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
   public async Task endpoint_returns_empty_when_authorisation_header_is_invalid(IAuthorization authorization)
   {
      var httpResponse = await InvokeAsync(keyId: _keyId, plaintext: "Zm9v", authorization: authorization);

      var content = await httpResponse.Content.ReadAsStringAsync();

      Assert.That(content, Is.Empty);
   }

   private async Task<HttpResponseMessage> InvokeAsync(
      Guid keyId,
      string plaintext,
      IAuthorization? authorization = null)
   {
      return await InvokeAsync(
         "Encrypt",
         new { KeyId = keyId, Plaintext = plaintext },
         authorization: authorization ?? new DefaultAuthorization(Region));
   }
}
