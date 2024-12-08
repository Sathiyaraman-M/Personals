using Expensive.Common.Wrappers;
using System.Text.Json.Serialization;

namespace Expensive.Common.Serialization;

[JsonSerializable(typeof(ValidationFailedResult))]
[JsonSerializable(typeof(ObjectNotFoundResult))]
[JsonSerializable(typeof(GenericFailedResult))]
[JsonSerializable(typeof(SuccessfulResult))]
public partial class ResultSerializationContext : JsonSerializerContext;