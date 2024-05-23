namespace HackerWebAPI.Interface
{
    public interface IHackerNewsService
    {
        Task<(int total, IEnumerable<object> newStories)> GetNewStoriesAsync(int page, int pageSize);
    }
}
