using Expensive.Common.Wrappers.Abstractions;
using System.Text.Json.Serialization;

namespace Expensive.Common.Wrappers;

public class ObjectNotFoundResult : IFailedResult
{
    public List<string> Messages { get; } = [];
    public bool Succeeded => false;

    public ObjectNotFoundResult() { }

    protected ObjectNotFoundResult(string message)
    {
        Messages.Add(message);
    }

    protected ObjectNotFoundResult(IEnumerable<string> messages)
    {
        Messages.AddRange(messages);
    }

    public static IResult Fail()
    {
        return new ObjectNotFoundResult();
    }

    public static IResult Fail(string message)
    {
        return new ObjectNotFoundResult(message);
    }

    public static IResult Fail(List<string> messages)
    {
        return new ObjectNotFoundResult(messages);
    }
}

public class ObjectNotFoundResult<T> : ObjectNotFoundResult, IFailedResult<T>
{
    [JsonIgnore] public T Data => default!;

    public ObjectNotFoundResult() { }

    protected ObjectNotFoundResult(string message) : base(message) { }

    protected ObjectNotFoundResult(IEnumerable<string> messages) : base(messages) { }

    public static new IResult<T> Fail()
    {
        return new ObjectNotFoundResult<T>();
    }

    public static new IResult<T> Fail(string message)
    {
        return new ObjectNotFoundResult<T>(message);
    }

    public static new IResult<T> Fail(List<string> messages)
    {
        return new ObjectNotFoundResult<T>(messages);
    }
}