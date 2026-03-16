namespace DotnetApiDddTemplate.Domain.Customers.Errors;

public static class CustomerErrors
{
    public static readonly Error NameRequired = new(
        "Customer.NameRequired",
        "Customer name is required");

    public static readonly Error NameTooLong = new(
        "Customer.NameTooLong",
        "Customer name must not exceed 200 characters");

    public static readonly Error EmailRequired = new(
        "Customer.EmailRequired",
        "Customer email is required");

    public static readonly Error InvalidEmail = new(
        "Customer.InvalidEmail",
        "Customer email format is invalid");

    public static readonly Error NotFound = new(
        "Customer.NotFound",
        "Customer not found");

    public static readonly Error AlreadyExists = new(
        "Customer.AlreadyExists",
        "Customer with this email already exists");

    public static readonly Error Deleted = new(
        "Customer.Deleted",
        "Customer has been deleted");
}
