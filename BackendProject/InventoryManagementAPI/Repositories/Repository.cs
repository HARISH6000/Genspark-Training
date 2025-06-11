using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;

namespace InventoryManagementAPI.Repositories
{
    public  abstract class Repository<K, T> : IRepository<K, T> where T:class
    {
        protected readonly ApplicationDbContext _applicationDbContext;

        public Repository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<T> Add(T item)
        {
            _applicationDbContext.Add(item);
            await _applicationDbContext.SaveChangesAsync();
            return item;
        }

        public async Task<T?> Delete(K key)
        {
            var item = await Get(key);
            if (item != null)
            {
                _applicationDbContext.Remove(item);
                await _applicationDbContext.SaveChangesAsync();
                return item;
            }
            throw new Exception("No such item found for deleting");
        }

        public abstract Task<T> Get(K key);


        public abstract Task<IEnumerable<T>> GetAll();


        public async Task<T> Update(K key, T item)
        {
            var myItem = await Get(key);
            if (myItem != null)
            {
                _applicationDbContext.Entry(myItem).CurrentValues.SetValues(item);
                await _applicationDbContext.SaveChangesAsync();
                return item;
            }
            throw new Exception("No such item found for updation");
        }
    }
}
