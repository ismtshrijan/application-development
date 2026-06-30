using Microsoft.EntityFrameworkCore;

namespace ElevateWorkforceSolutions.Models.ViewModels;

/// <summary>
/// Encapsulates pagination metadata and a page of items.
/// Inherits <see cref="List{T}"/> so it can be used directly as a model or composed within another ViewModel.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public class PaginatedList<T> : List<T>
{
    /// <summary>Current page index (1-based).</summary>
    public int PageIndex { get; private set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; private set; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; private set; }

    /// <summary>Initializes a new instance of <see cref="PaginatedList{T}"/>.</summary>
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        AddRange(items);
    }

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>Whether a next page exists.</summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Creates a <see cref="PaginatedList{T}"/> from an <see cref="IQueryable{T}"/> source
    /// by applying Skip/Take and counting asynchronously.
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
