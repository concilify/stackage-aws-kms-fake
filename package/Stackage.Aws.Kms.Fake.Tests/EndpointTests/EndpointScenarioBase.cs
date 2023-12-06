using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stackage.Aws.Kms.Fake.Services;
using Stackage.Aws.Kms.Fake.Tests.Stubs;

namespace Stackage.Aws.Kms.Fake.Tests.EndpointTests;

public abstract class EndpointScenarioBase
{
   private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = null };

   private StubGuidGenerator? _guidGenerator;
   private StubKeyStore? _keyStore;
   private WebApplicationFactory<Program>? _application;
   private HttpClient? _httpClient;

   [OneTimeSetUp]
   public void setup_once_before_all_tests()
   {
      _guidGenerator = new StubGuidGenerator();
      _keyStore = new StubKeyStore();

      _application = new WebApplicationFactory<Program>()
         .WithWebHostBuilder(webHostBuilder =>
         {
            webHostBuilder.ConfigureLogging(loggingBuilder =>
            {
               loggingBuilder.AddConsole();
            });
            webHostBuilder.ConfigureServices(services =>
            {
               services.AddSingleton<IGenerateGuids>(GuidGenerator);
               services.AddSingleton<IKeyStore>(KeyStore);
            });
         });
      _httpClient = _application.CreateClient();
   }

   [OneTimeTearDown]
   public void teardown_once_after_all_tests()
   {
      _application?.Dispose();
      _httpClient?.Dispose();
   }

   [SetUp]
   public void setup_before_each_test()
   {
      GuidGenerator.Clear();
      KeyStore.Clear();

      SetupBeforeEachTest();
   }

   protected StubGuidGenerator GuidGenerator => _guidGenerator ?? throw new InvalidOperationException("Scenario has not been initialised.");

   protected StubKeyStore KeyStore => _keyStore ?? throw new InvalidOperationException("Scenario has not been initialised.");

   protected HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("Scenario has not been initialised.");

   protected virtual void SetupBeforeEachTest()
   {
   }

   protected async Task<HttpResponseMessage> InvokeAsync(
      string target,
      IAuthorization? authorization = null)
   {
      return await InvokeAsync(target, new { }, authorization: authorization);
   }

   protected async Task<HttpResponseMessage> InvokeAsync<TRequest>(
      string target,
      TRequest request,
      IAuthorization? authorization = null)
   {
      authorization ??= new DefaultAuthorization();

      var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/")
      {
         Content = JsonContent.Create(request, new MediaTypeHeaderValue("application/x-amz-json-1.1"), SerializerOptions),
         Headers =
         {
            Authorization = authorization.Resolve()
         }
      };

      httpRequest.Headers.Add("X-Amz-Target", $"TrentService.{target}");

      return await HttpClient.SendAsync(httpRequest);
   }

   protected static async Task<JsonNode> ReadAsJsonNode(HttpResponseMessage httpResponse)
   {
      var contentJson = await httpResponse.Content.ReadAsStringAsync();

      var content = JsonNode.Parse(contentJson);

      if (content == null)
      {
         throw new InvalidOperationException("Response was empty.");
      }

      return content;
   }

   protected static TestCaseData[] InvalidAuthorizationHeaderTestCases()
   {
      return new[]
      {
         new TestCaseData(new MissingAuthorization())
            .SetName("Missing"),
         new TestCaseData(new SchemeParameterAuthorization("InvalidScheme", "InvalidParameter"))
            .SetName("Invalid")
      };
   }

   public interface IAuthorization
   {
      AuthenticationHeaderValue? Resolve();
   }

   protected record DefaultAuthorization(string Region = "ValidRegion") : IAuthorization
   {
      public AuthenticationHeaderValue? Resolve()
      {
         return new AuthenticationHeaderValue(
            "AWS4-HMAC-SHA256",
            $"Credential=ValidAwsSecretId/20001231/{Region}/kms/aws4_request, SignedHeaders=ValidSignedHeaders, Signature=ValidSignature");
      }
   }

   protected record SchemeParameterAuthorization(string Scheme, string Parameter) : IAuthorization
   {
      public AuthenticationHeaderValue? Resolve()
      {
         return new AuthenticationHeaderValue(Scheme, Parameter);
      }
   }

   protected record MissingAuthorization : IAuthorization
   {
      public AuthenticationHeaderValue? Resolve()
      {
         return null;
      }
   }
}
