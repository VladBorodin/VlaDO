using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VlaDO.Models;

namespace VlaDO.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DocumentFlowContext _context;
        private readonly DbSet<T> _set;

        public GenericRepository(DocumentFlowContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        // ────────── Чтение
        public async Task<IEnumerable<T>> GetAllAsync() => (IEnumerable<T>)await _set.ToListAsync();

        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] inc)
        {
            IQueryable<T> q = _set;
            q = inc.Aggregate(q, (cur, i) => cur.Include(i));
            return await q.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int page, int size) => (IEnumerable<T>)await _set
           .Skip((page - 1) * size)
           .Take(size)
           .ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> pred, Func<IQueryable<T>,
            IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] include)
        {
            IQueryable<T> q = _set;
            q = include.Aggregate(q, (cur, i) => cur.Include(i));
            q = q.Where(pred);
            if (orderBy != null) q = orderBy(q);
            return await q.ToListAsync();
        }
        public Task<bool> ExistsAsync(Guid id) =>
            _set.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);

        // ────────── Изменение (без SaveChanges)
        public Task AddAsync(T e) { _set.Add(e); return Task.CompletedTask; }
        public Task AddRangeAsync(IEnumerable<T> en) { _set.AddRange(en); return Task.CompletedTask; }
        public Task UpdateAsync(T e) { _set.Update(e); return Task.CompletedTask; }
        public Task UpdateRangeAsync(IEnumerable<T> en) { _set.UpdateRange(en); return Task.CompletedTask; }
        public async Task DeleteAsync(Guid id)
        {
            var e = await _set.FindAsync(id);
            if (e != null) _set.Remove(e);
        }
        public async Task<RoomUser?> GetByUserAndRoomAsync(Guid userId, Guid roomId)
        {
            return await _context.RoomUsers
                .FirstOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _set.AnyAsync(predicate);
        }

    }
}
