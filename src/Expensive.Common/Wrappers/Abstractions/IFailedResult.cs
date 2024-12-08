namespace Expensive.Common.Wrappers.Abstractions;

public interface IFailedResult : IResult
{
    new bool Succeeded => false;

    static abstract IResult Fail();

    static abstract IResult Fail(string message);

    static abstract IResult Fail(List<string> messages);
}

public interface IFailedResult<out T> : IFailedResult, IResult<T>
{
    static new abstract IResult<T> Fail();

    static new abstract IResult<T> Fail(string message);

    static new abstract IResult<T> Fail(List<string> messages);
}