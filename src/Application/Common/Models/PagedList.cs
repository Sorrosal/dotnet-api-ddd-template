namespace DotnetApiDddTemplate.Application.Common.Models;

/// <summary>
/// Represents a paginated list of items.
/// </summary>
public sealed record PagedList<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Has previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Has next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
