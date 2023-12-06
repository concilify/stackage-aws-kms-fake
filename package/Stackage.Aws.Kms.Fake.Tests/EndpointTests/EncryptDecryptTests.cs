using System;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public class EncryptDecryptTests : EndpointScenarioBase
{
   private static readonly Guid KeyAId = Guid.Parse("d466b10a-e905-4aa5-a060-c43c42c6fa5f");
   private static readonly Guid KeyBId = Guid.Parse("80bd55f9-ca49-4bdf-ae9f-9313b171c596");

   private static readonly byte[] BackingKeyA = Convert.FromHexString("879b83ce4de6ca25ada96062c885b9ba0adce98ce449a4805c15523bf9e5dfbb");
   private static readonly byte[] BackingKeyB = Convert.FromHexString("b1546a53d72aa97316b50a6f08e99f33ac11811eb4b6fc5e4250ec346d444747");

   private const string Region = "ArbitraryRegion";

   protected override void SetupBeforeEachTest()
   {
      KeyStore.Seed(
         Key.Create(id: KeyAId, region: Region, aliases: new[] { "alias/A" }, backingKey: BackingKeyA),
         Key.Create(id: KeyBId, region: Region, aliases: new[] { "alias/B" }, backingKey: BackingKeyB));
   }

   [TestCaseSource(nameof(RoundTripTestCases))]
   public async Task encrypted_content_can_be_decrypted(string keyId, string plaintext)
   {
      var plaintextBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(plaintext));

      var encryptHttpResponse = await InvokeAsync(
         "Encrypt",
         new { KeyId = keyId, Plaintext = plaintextBase64 });

      var encryptContent = await ReadAsJsonNode(encryptHttpResponse);

      var ciphertextBlob = encryptContent["CiphertextBlob"]?.GetValue<string>();

      var decryptHttpResponse = await InvokeAsync(
         "Decrypt",
         new { CiphertextBlob = ciphertextBlob });

      var decryptContent = await ReadAsJsonNode(decryptHttpResponse);

      Assert.That(decryptContent["Plaintext"]?.GetValue<string>(), Is.EqualTo(plaintextBase64));
   }

   private static TestCaseData[] RoundTripTestCases()
   {
      return new[]
      {
         new TestCaseData(KeyAId.ToString(), "Hello World!"),
         new TestCaseData(KeyAId.ToString(), "The quick brown fox jumps over the lazy dog"),
         new TestCaseData(KeyBId.ToString(), "Hello World!"),
         new TestCaseData(KeyBId.ToString(), "The quick brown fox jumps over the lazy dog"),
         new TestCaseData("alias/A", "Hello World!"),
         new TestCaseData("alias/A", "The quick brown fox jumps over the lazy dog"),
         new TestCaseData("alias/B", "Hello World!"),
         new TestCaseData("alias/B", "The quick brown fox jumps over the lazy dog")
      };
   }
}
