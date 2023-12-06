using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public class CreateAliasTests : EndpointScenarioBase
{
   private static readonly Guid KeyId = Guid.Parse("ce842197-7d02-467d-8913-aba77627b62e");

   private const string Region = "ArbitraryRegion";

   protected override void SetupBeforeEachTest()
   {
      KeyStore.Seed(Key.Create(id: KeyId, region: Region));
   }

   [Test]
   public async Task endpoint_returns_200_okay()
   {
      var httpResponse = await InvokeCreateAliasAsync(aliasName: "alias/ValidAliasName", targetKeyId: KeyId);

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
   }

   [Test]
   public async Task endpoint_returns_empty_content()
   {
      var httpResponse = await InvokeCreateAliasAsync(aliasName: "alias/ValidAliasName", targetKeyId: KeyId);
      var content = await ReadAsJsonNode(httpResponse);

      Assert.That(content.ToJsonString(), Is.EqualTo("{}"));
   }

   [Test]
   public async Task endpoint_returns_content_type()
   {
      var httpResponse = await InvokeCreateAliasAsync(aliasName: "alias/ValidAliasName", targetKeyId: KeyId);

      Assert.That(httpResponse.Content.Headers.ContentType?.ToString(), Is.EqualTo("application/x-amz-json-1.1"));
   }

   [Test]
   public async Task endpoint_returns_request_id()
   {
      var awsRequestId = Guid.NewGuid();

      GuidGenerator.Seed(awsRequestId);

      var httpResponse = await InvokeCreateAliasAsync(aliasName: "alias/ValidAliasName", targetKeyId: KeyId);

      Assert.That(httpResponse.Headers.GetValues("X-Amzn-RequestId").Single(), Is.EqualTo(awsRequestId.ToString()));
   }

   [Test]
   public async Task endpoint_adds_alias_to_existing_key_in_store()
   {
      await InvokeCreateAliasAsync(aliasName: "alias/ArbitraryAliasName", targetKeyId: KeyId);

      Assert.That(KeyStore.Keys.Count, Is.EqualTo(1));
      Assert.That(KeyStore.Keys[0].Aliases, Is.EquivalentTo(new[] { "alias/ArbitraryAliasName" }));
   }

   [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
   public async Task endpoint_returns_401_unauthorized_when_authorisation_header_is_invalid(IAuthorization authorization)
   {
      var httpResponse = await InvokeCreateAliasAsync(aliasName: "alias/ValidAliasName", targetKeyId: KeyId, authorization: authorization);

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
   }

   [TestCaseSource(nameof(InvalidAuthorizationHeaderTestCases))]
   public async Task endpoint_returns_empty_when_authorisation_header_is_invalid(IAuthorization authorization)
   {
      var httpResponse = await InvokeCreateAliasAsync(aliasName: "alias/ValidAliasName", targetKeyId: KeyId, authorization: authorization);

      var content = await httpResponse.Content.ReadAsStringAsync();

      Assert.That(content, Is.Empty);
   }

   private async Task<HttpResponseMessage> InvokeCreateAliasAsync(
      string aliasName,
      Guid targetKeyId,
      IAuthorization? authorization = null)
   {
      return await InvokeAsync(
         "CreateAlias",
         new { AliasName = aliasName, TargetKeyId = targetKeyId },
         authorization: authorization ?? new DefaultAuthorization(Region));
   }
}
