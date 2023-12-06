using System;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Model;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public class DecryptTests : EndpointScenarioBase
{
   private static readonly Guid KeyId = Guid.Parse("1abf1c7b-4280-4226-89c2-b91a53dc4786");
   private static readonly byte[] BackingKey = Convert.FromHexString("0492a13a9d25043d48c48fd540cbeade45ba069bfae07ddb8363114110fb83ec");

   private const string Region = "ArbitraryRegion";

   protected override void SetupBeforeEachTest()
   {
      KeyStore.Seed(Key.Create(id: KeyId, region: Region, backingKey: BackingKey));
   }

   [Test]
   public async Task endpoint_returns_200_okay()
   {
      var httpResponse = await InvokeAsync(
         "Decrypt", new { CiphertextBlob = "" });

      Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
   }

   // TODO: Different region
   // TODO: Missing id
}
