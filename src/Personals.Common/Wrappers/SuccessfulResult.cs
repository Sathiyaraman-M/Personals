using Personals.Common.Wrappers.Abstractions;

namespace Personals.Common.Wrappers;

public class SuccessfulResult : ISuccessfulResult
{
    public List<string> Messages { get; protected init; }

    public bool Succeeded => true;

    public SuccessfulResult()
    {
        Messages = [];
    }

    private SuccessfulResult(string message)
    {
        Messages = [message];
    }

    private SuccessfulResult(IEnumerable<string> messages)
    {
        Messages = messages.ToList();
    }

    public static IResult Succeed()
    {
        return new SuccessfulResult();
    }

    public static IResult Succeed(string message)
    {
        return new SuccessfulResult(message);
    }

    public static IResult Succeed(List<string> messages)
    {
        return new SuccessfulResult(messages);
    }
}

public class SuccessfulResult<T> : SuccessfulResult, ISuccessfulResult<T>
{
    public T Data { get; set; }

    public SuccessfulResult()
    {
        Data = default!;
    }

    protected SuccessfulResult(T data)
    {
        Data = data;
    }

    private SuccessfulResult(T data, string message)
    {
        Data = data;
        Messages = [message];
    }

    private SuccessfulResult(T data, IEnumerable<string> messages)
    {
        Data = data;
        Messages = messages.ToList();
    }

    public static IResult<T> Succeed(T data)
    {
        return new SuccessfulResult<T>(data);
    }

    public static IResult<T> Succeed(T data, string message)
    {
        return new SuccessfulResult<T>(data, message);
    }

    public static IResult<T> Succeed(T data, List<string> messages)
    {
        return new SuccessfulResult<T>(data, messages);
    }
}