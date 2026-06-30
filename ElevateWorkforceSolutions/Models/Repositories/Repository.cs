using Microsoft.EntityFrameworkCore;
using ElevateWorkforceSolutions.Data;

namespace ElevateWorkforceSolutions.Models.Repositories;

/// <summary>Generic repository implementation wrapping <see cref="ApplicationDbContext"/>.</summary>
/// <typeparam name="T">Entity type.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    /// <summary>Initializes a new instance of <see cref="Repository{T}"/>.</summary>
    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await DbSet.ToListAsync();

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    /// <inheritdoc />
    public virtual void Update(T entity) => DbSet.Update(entity);

    /// <inheritdoc />
    public virtual void Delete(T entity) => DbSet.Remove(entity);

    /// <inheritdoc />
    public virtual async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
