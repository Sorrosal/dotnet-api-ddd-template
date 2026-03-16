namespace DotnetApiDddTemplate.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Handler for UpdateCustomerCommand.
/// </summary>
public sealed class UpdateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCustomerCommandHandler> logger) : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Updating customer {CustomerId}",
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

            // Check if new email is already used by another customer
            if (customer.Email != request.Email)
            {
                var emailExists = await customerRepository.ExistsByEmailAsync(request.Email, cancellationToken);
                if (emailExists)
                {
                    logger.LogWarning("Email {Email} is already in use", request.Email);
                    return Result.Failure(CustomerErrors.AlreadyExists);
                }
            }

            // Update customer
            var result = customer.Update(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.City,
                request.Country);

            if (result.IsFailure)
            {
                logger.LogWarning("Failed to update customer: {Error}", result.Error.Message);
                return result;
            }

            // Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Customer {CustomerId} updated successfully", request.CustomerId);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency error while updating customer {CustomerId}", request.CustomerId);
            return Result.Failure(new Error("Concurrency.Conflict", "A concurrency conflict occurred"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
