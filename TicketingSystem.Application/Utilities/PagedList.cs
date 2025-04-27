namespace TicketingSystem.Application.Utilities
{
    public class PagedList<T>(IEnumerable<T> data, int currentPage, int pageSize, int totalCount)
    {
        public IEnumerable<T> Data { get; private set; } = data;
        public int CurrentPage { get; private set; } = currentPage;
        public int PageSize { get; private set; } = pageSize;
        public int TotalCount { get; private set; } = totalCount;
        public int TotalPages { get; private set; } = (int)Math.Ceiling((double)totalCount / pageSize);
    }
}
