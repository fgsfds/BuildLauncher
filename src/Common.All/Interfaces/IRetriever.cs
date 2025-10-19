namespace Common.All.Interfaces;

public interface IRetriever<T>
{
    Task<T?> RetrieveAsync();
}
