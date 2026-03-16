namespace DotnetApiDddTemplate.Application.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Handler for CreateCustomerCommand.
/// </summary>
public sealed class CreateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCustomerCommandHandler> logger) : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating customer with email {Email}",
            request.Email);

        try
        {
            // Check if email already exists
            var exists = await customerRepository.ExistsByEmailAsync(request.Email, cancellationToken);
            if (exists)
            {
                logger.LogWarning("Customer with email {Email} already exists", request.Email);
                return Result<Guid>.Failure(CustomerErrors.AlreadyExists);
            }

            // Create customer
            var result = Customer.Create(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.City,
                request.Country);

            if (result.IsFailure)
            {
                logger.LogWarning("Failed to create customer: {Error}", result.Error.Message);
                return Result<Guid>.Failure(result.Error);
            }

            var customer = result.Value;

            // Add to repository
            await customerRepository.AddAsync(customer, cancellationToken);

            // Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Customer {CustomerId} created successfully",
                customer.Id.Value);

            return Result<Guid>.Success(customer.Id.Value);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency error while creating customer");
            return Result<Guid>.Failure(new Error("Concurrency.Conflict", "A concurrency conflict occurred"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating customer");
            throw;
        }
    }
}
