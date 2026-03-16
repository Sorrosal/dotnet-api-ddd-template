namespace DotnetApiDddTemplate.IntegrationTests.Api.Customers;

/// <summary>
/// Integration tests for CustomersController.
/// Tests complete HTTP pipeline with real database via Testcontainers.
/// </summary>
public sealed class CustomersControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _httpClient = null!;
    private ApplicationDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _httpClient = _factory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", Guid.NewGuid().ToString());

        using var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Post_Should_Create_When_ValidRequest()
    {
        // Arrange
        var request = new
        {
            name = "John Doe",
            email = "john@example.com",
            phoneNumber = "+1234567890",
            city = "New York"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/customers",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var jsonString = await response.Content.ReadAsStringAsync();
        var customerId = System.Text.Json.JsonSerializer.Deserialize<Guid>(jsonString);
        customerId.Should().NotBeEmpty();

        // Verify database
        var customer = await _dbContext.Customers.FindAsync(new CustomerId(customerId));
        customer.Should().NotBeNull();
        customer!.Name.Should().Be(request.name);
        customer.Email.Should().Be(request.email);
    }

    [Fact]
    public async Task Get_Should_Return_When_Exists()
    {
        // Arrange - create customer
        var customer = Customer.Create("John Doe", "john@example.com", city: "New York").Value;
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync($"/api/v1/customers/{customer.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = System.Text.Json.JsonSerializer.Deserialize<GetCustomerByIdResponse>(jsonString);
        content!.Name.Should().Be(customer.Name);
        content.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task Get_Should_ReturnNotFound_When_DoesNotExist()
    {
        // Act
        var response = await _httpClient.GetAsync($"/api/v1/customers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(jsonString);
        content!.Code.Should().Be("Customer.NotFound");
    }

    [Fact]
    public async Task GetList_Should_ReturnPaginated()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var customer = Customer.Create($"Customer {i}", $"customer{i}@example.com").Value;
            _dbContext.Customers.Add(customer);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.GetAsync("/api/v1/customers?pageNumber=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = System.Text.Json.JsonSerializer.Deserialize<PagedListResponse>(jsonString);
        content!.Items.Should().HaveCount(3);
        content.TotalCount.Should().Be(5);
        content.PageNumber.Should().Be(1);
        content.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Put_Should_Update_When_Exists()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var request = new
        {
            name = "Jane Doe",
            email = "jane@example.com",
            city = "Los Angeles"
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/customers/{customer.Id.Value}",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify database
        var updated = await _dbContext.Customers.FindAsync(customer.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(request.name);
        updated.Email.Should().Be(request.email);
    }

    [Fact]
    public async Task Delete_Should_SoftDelete_When_Exists()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1/customers/{customer.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete
        var deleted = await _dbContext.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == customer.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_Should_ReturnBadRequest_When_NameEmpty()
    {
        // Arrange
        var request = new
        {
            name = "",
            email = "john@example.com"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/customers",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_Should_ReturnBadRequest_When_DuplicateEmail()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@example.com").Value;
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var request = new
        {
            name = "Jane Doe",
            email = "john@example.com"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/customers",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(jsonString);
        content!.Code.Should().Be("Customer.AlreadyExists");
    }
}

/// <summary>
/// Helper type for paged list deserialization in tests.
/// </summary>
public sealed record PagedListResponse(
    List<object> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
