using System;

namespace Stackage.Aws.Kms.Fake.Model;

public class Key
{
   private const string DefaultRegion = "eu-central-1";

   private Key(Guid id, string region, DateTime createdAt)
   {
      Id = id;
      Region = region;
      CreatedAt = createdAt;
   }

   public Guid Id { get; }

   public string Region { get; }

   public DateTime CreatedAt { get; }

   public string Arn => $"arn:aws:kms:{Region}:000000000000:key/{Id}";

   public static Key Create(
      Guid? id = null,
      string? region = null)
   {
      return new Key(id ?? new Guid(), region ?? DefaultRegion, DateTime.Now);
   }
}
