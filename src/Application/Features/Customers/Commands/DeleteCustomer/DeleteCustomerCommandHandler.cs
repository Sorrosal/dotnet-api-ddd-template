namespace DotnetApiDddTemplate.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Handler for DeleteCustomerCommand.
/// Performs soft delete by marking customer as deleted.
/// </summary>
public sealed class DeleteCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCustomerCommandHandler> logger) : IRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCustomerCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Deleting customer {CustomerId}",
            request.CustomerId);

        try
        {
            var customerId = new CustomerId(request.CustomerId);
            var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);

            if (customer is null)
            {
                logger.LogWarning("Customer {CustomerId} not found", request.CustomerId);
                return Result.Failure(CustomerErrors.NotFound);
            }

            // Delete customer (soft delete)
            var result = customer.Delete();
            if (result.IsFailure)
            {
                logger.LogWarning("Failed to delete customer: {Error}", result.Error.Message);
                return result;
            }

            // Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Customer {CustomerId} deleted successfully", request.CustomerId);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency error while deleting customer {CustomerId}", request.CustomerId);
            return Result.Failure(new Error("Concurrency.Conflict", "A concurrency conflict occurred"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
