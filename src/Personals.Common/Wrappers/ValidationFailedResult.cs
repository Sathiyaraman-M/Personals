using Personals.Common.Wrappers.Abstractions;
using System.Text.Json.Serialization;

namespace Personals.Common.Wrappers;

public class ValidationFailedResult : IFailedResult
{
    public List<string> Messages { get; init; } = [];
    public bool Succeeded => false;

    public ValidationFailedResult()
    {
    }

    protected ValidationFailedResult(string message)
    {
        Messages.Add(message);
    }

    protected ValidationFailedResult(IEnumerable<string> messages)
    {
        Messages.AddRange(messages);
    }

    public static IResult Fail()
    {
        return new ValidationFailedResult();
    }

    public static IResult Fail(string message)
    {
        return new ValidationFailedResult(message);
    }

    public static IResult Fail(List<string> messages)
    {
        return new ValidationFailedResult(messages);
    }
}

public class ValidationFailedResult<T> : ValidationFailedResult, IFailedResult<T>
{
    [JsonIgnore] public T Data => default!;

    public ValidationFailedResult() { }

    private ValidationFailedResult(string message) : base(message) { }

    private ValidationFailedResult(IEnumerable<string> messages) : base(messages) { }

    public static new IResult<T> Fail()
    {
        return new ValidationFailedResult<T>();
    }

    public static new IResult<T> Fail(string message)
    {
        return new ValidationFailedResult<T>(message);
    }

    public static new IResult<T> Fail(List<string> messages)
    {
        return new ValidationFailedResult<T>(messages);
    }
}