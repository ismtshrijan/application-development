namespace ElevateWorkforceSolutions.Models.Repositories;

/// <summary>Generic repository interface for CRUD operations.</summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Gets an entity by its primary key.</summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>Gets all entities.</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Adds a new entity.</summary>
    Task AddAsync(T entity);

    /// <summary>Marks an entity as updated.</summary>
    void Update(T entity);

    /// <summary>Marks an entity as deleted.</summary>
    void Delete(T entity);

    /// <summary>Persists all pending changes to the database.</summary>
    Task SaveChangesAsync();
}
