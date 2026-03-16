namespace DotnetApiDddTemplate.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Customer aggregate root.
/// Provides data access with soft delete filtering and specification support.
/// </summary>
public sealed class CustomerRepository(
    ApplicationDbContext context,
    ILogger<CustomerRepository> logger) : Repository<Customer, CustomerId>(context, logger), IPagedCustomerRepository
{
    public async Task<Customer?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving customer by email: {Email}", email);

        var customer = await Context.Customers
            .Where(c => c.Email == email && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            logger.LogWarning("Customer with email {Email} not found", email);

        return customer;
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if customer exists by email: {Email}", email);

        return await Context.Customers
            .Where(c => c.Email == email && !c.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all customers");

        return await Context.Customers
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<Customer>> GetPagedAsync(
        ISpecification<Customer> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Retrieving paged customers: PageNumber={PageNumber}, PageSize={PageSize}",
            pageNumber,
            pageSize);

        // Cast specification to CustomersBySearchSpecification to apply filters
        var query = Context.Customers.AsQueryable();

        if (specification is CustomersBySearchSpecification searchSpec)
        {
            query = searchSpec.Apply(query);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved {Count} customers (total: {Total})",
            items.Count,
            totalCount);

        return new PagedList<Customer>(items, totalCount, pageNumber, pageSize);
    }
}
