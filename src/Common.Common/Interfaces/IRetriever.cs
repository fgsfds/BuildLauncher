namespace Common.Common.Interfaces;

public interface IRetriever<T>
{
    public Task<T?> RetrieveAsync();
}
