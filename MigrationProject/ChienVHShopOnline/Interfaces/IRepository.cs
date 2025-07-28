namespace ChienVHShopOnline.Interfaces
{
    public interface IRepository<K, T> where T : class
    {
        Task<T> Add(T item);
        Task<T?> Delete(K key);
        Task<T> Update(K key, T item);
        Task<T?> Get(K key);
        Task<IEnumerable<T>> GetAll();
    }
}
