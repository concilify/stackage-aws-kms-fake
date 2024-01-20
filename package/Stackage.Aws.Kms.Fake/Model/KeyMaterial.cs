using System;
using System.Collections.Immutable;

namespace Stackage.Aws.Kms.Fake.Model;

public record KeyMaterial(ReadOnlySpan<byte> Bytes);
