using System.Security.Authentication;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Stackage.Aws.Kms.Fake.Services;

public class AuthenticationContext : IAuthenticationContext
{
   private static readonly Regex CredentialRegex =
      new(@"Credential=(?<aws_secret_id>.*?)\/(?<date>[0-9]{8})\/(?<region>.*?)\/kms\/aws4_request");

   private readonly string? _region;

   public AuthenticationContext(IHttpContextAccessor httpContextAccessor)
   {
      _region = GetRegion(httpContextAccessor.HttpContext);
   }

   public bool IsAuthenticated => _region != null;

   public string Region => _region ?? throw new AuthenticationException("Request is not authenticated");

   private static string? GetRegion(HttpContext? context)
   {
      if (context == null)
      {
         return null;
      }

      var headers = context.Request.Headers;

      var credentialMatch = CredentialRegex.Match(headers.Authorization.ToString());

      return credentialMatch.Success ? credentialMatch.Groups["region"].Value : null;
   }
}
