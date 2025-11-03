using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taskly.Application.Interfaces;
using Taskly.Domain; // For ApplicationDbContext
using Taskly.Domain.Entities; // For BaseEntity

namespace Taskly.Infrastructure.Services
{
    public class EntityService<T> : IEntityService<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public EntityService(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllAsync(T entity)
        {
            _dbSet.RemoveRange(_dbSet);
            await _context.SaveChangesAsync();
        }
    }
}
