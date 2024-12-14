namespace Personals.Common.Wrappers;

public class PaginatedResult<T> : SuccessfulResult<List<T>>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PaginatedResult()
    {
    }

    private PaginatedResult(List<T> data, int currentPage, int pageSize, long totalCount) : base(data)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PaginatedResult<T> Create(List<T> data, int currentPage, int pageSize, int totalCount)
    {
        return new PaginatedResult<T>(data, currentPage, pageSize, totalCount);
    }

    public static PaginatedResult<T> Create(List<T> data, int currentPage, int pageSize, int totalCount, string message)
    {
        return new PaginatedResult<T>(data, currentPage, pageSize, totalCount) { Messages = [message] };
    }

    public static PaginatedResult<T> Create(List<T> data, int currentPage, int pageSize, int totalCount,
        IEnumerable<string> messages)
    {
        return new PaginatedResult<T>(data, currentPage, pageSize, totalCount) { Messages = messages.ToList() };
    }

    public static PaginatedResult<T> Create(List<T> data, int currentPage, int pageSize, long totalCount)
    {
        return new PaginatedResult<T>(data, currentPage, pageSize, totalCount);
    }

    public static PaginatedResult<T> Create(List<T> data, int currentPage, int pageSize, long totalCount, string message)
    {
        return new PaginatedResult<T>(data, currentPage, pageSize, totalCount) { Messages = [message] };
    }

    public static PaginatedResult<T> Create(List<T> data, int currentPage, int pageSize, long totalCount,
        IEnumerable<string> messages)
    {
        return new PaginatedResult<T>(data, currentPage, pageSize, totalCount) { Messages = messages.ToList() };
    }
}