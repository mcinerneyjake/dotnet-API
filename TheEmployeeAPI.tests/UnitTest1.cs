using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TheEmployeeAPI.Abstractions;

namespace TheEmployeeAPI.Tests;

public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _factory;

  public BasicTests(WebApplicationFactory<Program> factory)
  {
    _factory = factory;

    var repo = _factory.Services.GetRequiredService<IRepository<Employee>>();
    repo.Create(new Employee { FirstName = "John", LastName = "Doe" });
  }

  [Fact]
  public async Task GetAllEmployees_ReturnsOkResult()
  {
    HttpClient client = _factory.CreateClient();
    var response = await client.GetAsync("/employees");

    response.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task GetEmployeeById_ReturnsOkResult()
  {
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/employees/1");

    response.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task CreateEmployee_ReturnsCreatedResult()
  {
    var client = _factory.CreateClient();
    var response = await client.PostAsJsonAsync("/employees", new Employee {
      FirstName = "John",
      LastName = "Doe",
      SocialSecurityNumber = "123-46-7890",
    });

    response.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task CreateEmployee_ReturnsBadRequestResult()
  {
    var client = _factory.CreateClient();
    var response = await client.PostAsJsonAsync("/employees", new{});

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task UpdateEmployee_ReturnsNotFoundForNonExistentEmployee()
  {
      var client = _factory.CreateClient();
      var response = await client.PutAsJsonAsync("/employees/9999", new Employee { FirstName = "Bambi", LastName = "Doe", SocialSecurityNumber = "123-45-7892" });

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
