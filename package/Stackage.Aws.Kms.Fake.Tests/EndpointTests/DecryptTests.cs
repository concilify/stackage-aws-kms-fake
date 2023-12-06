using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public class DecryptTests : EndpointScenarioBase
{
   private static readonly Guid KeyId = Guid.Parse("1abf1c7b-4280-4226-89c2-b91a53dc4786");
   private static readonly byte[] BackingKey = Convert.FromHexString("0492a13a9d25043d48c48fd540cbeade45ba069bfae07ddb8363114110fb83ec");

   private const string MinimalCiphertextBlob = "exy/GoBCJkKJwrkaU9xHhqlb0D24Ab/SIY+Oe6PGw0mD60rxd4Mv9nmGbhM=";
   private const string Region = "ArbitraryRegion";

   protected override void SetupBeforeEachTest()
   {
      KeyStore.Seed(Key.Create(id: KeyId, region: Region, backingKey: BackingKey));
   }

   [Test]
   public async Task endpoint_returns_200_okay()
   {
      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: MinimalCiphertextBlob);

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
   }

   [TestCase("exy/GoBCJkKJwrkaU9xHhh6ZhVKJcmKpGHq1Dj61cwNANrJtm/vDGz+PbuUclS3oZj5iFgL+VEI=")]
   [TestCase("exy/GoBCJkKJwrkaU9xHhiKbk0uDEl30e+ng+J2vb2WUlZuH7Z764uYTTdq/xGrDGfRJsex8sMk=")]
   public async Task endpoint_returns_decrypted_content(string ciphertextBlob)
   {
      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: ciphertextBlob);

      var content = await ReadAsJsonNode(httpResponse);

      Assert.That(content?["Plaintext"]?.GetValue<string>(), Is.EqualTo("SGVsbG8gV29ybGQh"));
      Assert.That(content?["KeyId"]?.GetValue<string>(), Is.EqualTo($"arn:aws:kms:{Region}:000000000000:key/{KeyId}"));
   }

   [Test]
   public async Task endpoint_returns_content_type()
   {
      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: MinimalCiphertextBlob);

      Assert.That(httpResponse.Content.Headers.ContentType?.ToString(), Is.EqualTo("application/x-amz-json-1.1"));
   }

   [Test]
   public async Task endpoint_returns_request_id()
   {
      var awsRequestId = Guid.NewGuid();

      GuidGenerator.Seed(awsRequestId);

      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: MinimalCiphertextBlob);

      Assert.That(httpResponse.Headers.GetValues("X-Amzn-RequestId").Single(), Is.EqualTo(awsRequestId.ToString()));
   }

   [Test]
   public async Task endpoint_returns_400_when_key_not_found()
   {
      const string ciphertextBlobWithMissingKeyId = "ke3vyH6bmkeKVcNl/jG5BOYWSB3saKIAlmrFl6h/tZ2IZdKu1H99Ylds6t8QCSMJgbzkK+OedaM=";

      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: ciphertextBlobWithMissingKeyId);

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
   }

   [Test]
   public async Task endpoint_returns_message_when_key_not_found()
   {
      const string ciphertextBlobWithMissingKeyId = "ke3vyH6bmkeKVcNl/jG5BOYWSB3saKIAlmrFl6h/tZ2IZdKu1H99Ylds6t8QCSMJgbzkK+OedaM=";

      var httpResponse = await InvokeDecryptAsync(
         ciphertextBlob: ciphertextBlobWithMissingKeyId,
         authorization: new DefaultAuthorization(Region));

      var content = await ReadAsJsonNode(httpResponse);

      Assert.That(
         content["__type"]?.GetValue<string>(),
         Is.EqualTo("NotFoundException"));
      Assert.That(
         content["message"]?.GetValue<string>(),
         Is.EqualTo($"Key 'arn:aws:kms:{Region}:000000000000:key/c8efed91-9b7e-479a-8a55-c365fe31b904' does not exist"));
   }

   [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
   public async Task endpoint_returns_401_unauthorized_when_authorisation_header_is_invalid(IAuthorization authorization)
   {
      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: MinimalCiphertextBlob, authorization: authorization);

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
   }

   [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
   public async Task endpoint_returns_empty_when_authorisation_header_is_invalid(IAuthorization authorization)
   {
      var httpResponse = await InvokeDecryptAsync(ciphertextBlob: MinimalCiphertextBlob, authorization: authorization);

      var content = await httpResponse.Content.ReadAsStringAsync();

      Assert.That(content, Is.Empty);
   }

   private async Task<HttpResponseMessage> InvokeDecryptAsync(
      string ciphertextBlob,
      IAuthorization? authorization = null)
   {
      return await InvokeAsync(
         "Decrypt",
         new { CiphertextBlob = ciphertextBlob },
         authorization: authorization ?? new DefaultAuthorization(Region));
   }
}
