using ChienVHShopOnline.Data;
using ChienVHShopOnline.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T> Add(T item)
        {
            _context.Set<T>().Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<T?> Delete(K key)
        {
            var item = await Get(key);
            if (item != null)
            {
                _context.Set<T>().Remove(item);
                await _context.SaveChangesAsync();
                return item;
            }
            throw new Exception("No such item found for deleting");
        }

        public async Task<T> Update(K key, T item)
        {
            var existing = await Get(key);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
                return item;
            }
            throw new Exception("No such item found for updating");
        }

        public abstract Task<T?> Get(K key);
        public abstract Task<IEnumerable<T>> GetAll();
    }
}
