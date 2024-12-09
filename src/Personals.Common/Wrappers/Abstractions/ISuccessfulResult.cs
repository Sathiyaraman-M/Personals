namespace Personals.Common.Wrappers.Abstractions;

public interface ISuccessfulResult : IResult
{
    new bool Succeeded => true;

    static abstract IResult Succeed();

    static abstract IResult Succeed(string message);

    static abstract IResult Succeed(List<string> messages);
}

public interface ISuccessfulResult<T> : ISuccessfulResult, IResult<T>
{
    static abstract IResult<T> Succeed(T data);

    static abstract IResult<T> Succeed(T data, string message);

    static abstract IResult<T> Succeed(T data, List<string> messages);
}